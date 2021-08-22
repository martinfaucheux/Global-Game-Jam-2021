using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class AITargetManager : MonoBehaviour
{
    [SerializeField] private bool LogDebug = true;

    private Seeker seeker;
    private IAstarAI ai;
    private bool allowUpdates;

    // Calcs
    private double lastjobId = 0;
    private List<Job> runingJobs = new List<Job>();

    public class Job
    {
        private double jobId = 0;
        private int totalCalcs = 0;
        private int curCalcs = 0;
        private Dictionary<Escape, float> escapeDistances;
        private List<Escape> remainingCalcs;

        public double JobId => jobId;
        private Escape shortestEscape;
        public Escape ShortestEscape => shortestEscape;
        private float shortestDistance;
        public float ShortestDistance => shortestDistance;

        public event Action<Job> onJobComplete;

        public Job(double _jobId, List<Escape> _escapes)
        {
            jobId = _jobId;
            totalCalcs = _escapes.Count;
            escapeDistances = new Dictionary<Escape, float>();
            _escapes.ForEach(x => escapeDistances.Add(x, float.PositiveInfinity));

            remainingCalcs = new List<Escape>();
            _escapes.ForEach(x => remainingCalcs.Add(x));

            // Init
            shortestEscape = null;
            shortestDistance = Mathf.Infinity;
        }

        public void RegisterResult(Escape escape, float distance)
        {
            curCalcs++;
            // For testing
            if (escapeDistances == null || !escapeDistances.ContainsKey(escape))
            {
                Debug.LogError("Error in AITargetManager Job ");
                return;
            }

            escapeDistances[escape] = distance;
            
            // Calculate Shortest Target & Terminate Job
            if (curCalcs == totalCalcs)
            {
                calculateShortestEscape();

                Debug.Log(string.Format("Job {0} finished with {1} calcs. Shortest path is {2}",
                    JobId, totalCalcs, shortestEscape != null ? shortestEscape.name : "No Shortest Escape"));

                onJobComplete.Invoke(this);
            }
        }

        /// <summary>
        /// Get and remove next Escape from remaining Escape path calculation processing list
        /// </summary>
        public Escape PullRemainingCalc
        {
            get
            {
                if (remainingCalcs.Count > 0)
                {
                    Escape escape = remainingCalcs[remainingCalcs.Count - 1];
                    remainingCalcs.RemoveAt(remainingCalcs.Count - 1);
                    return escape;
                }
                else
                    return null;
            }
        }
        
        private void calculateShortestEscape()
        {
            foreach(Escape escape in escapeDistances.Keys)
            {
                if(escapeDistances[escape] < shortestDistance)
                {
                    shortestEscape = escape;
                    shortestDistance = escapeDistances[escape];
                }
            }
        }
    }

    void Awake()
    {
        seeker = GetComponent<Seeker>();
        ai = GetComponent<IAstarAI>();


        EscapesManager.onEscapeRegistered += UpdateTarget;
        EscapesManager.onEscapeUnRegistered += UpdateTarget;
        GameManager.onBlockRegistered += UpdateGraph;
        GameManager.onBlockUnRegistered += UpdateGraph;
        GameManager.onPlayerDie += Stop;
        GameEvents.instance.onGameOver += Stop;

        allowUpdates = true;

        // Update the destination right before searching for a path as well.
        // This is enough in theory, but this script will also update the destination every
        // frame as the destination is used for debugging and may be used for other things by other
        // scripts as well. So it makes sense that it is up to date every frame.
        // if (ai != null) ai.onSearchPath += UpdateTarget;
    }

    void OnDestroy()
    {
        EscapesManager.onEscapeRegistered -= UpdateTarget;
        EscapesManager.onEscapeUnRegistered -= UpdateTarget;
        GameManager.onBlockRegistered -= UpdateGraph;
        GameManager.onBlockUnRegistered -= UpdateGraph;
        GameManager.onPlayerDie -= Stop;
        GameEvents.instance.onGameOver -= Stop;

        allowUpdates = false;
        seeker.CancelCurrentPathRequest();
    }

    /// <summary>
    /// Look for each Escape to find closest reachable
    /// </summary>
    private void UpdateTarget()
    {
        if (ai == null || !allowUpdates)
            return;

        if(LogDebug) Debug.Log("Calculating Closest target");

        if (!EscapesManager.HasEscape)
            if(LogDebug) Debug.LogWarning("No escapes found");
        
        // Increment JobID
        lastjobId++;

        // Create a new Job
        Job job = new Job(lastjobId, EscapesManager.Escapes);
        // Keep track just in case
        runingJobs.Add(job);
        // Add callbacks upon job completion
        job.onJobComplete += updateDestination;
        job.onJobComplete += removeFromJobs;

        // Find closest Escape
        Escape firstEscape = job.PullRemainingCalc;
        if(firstEscape != null)
            seeker.StartPath(transform.position, firstEscape.gameObject.transform.position, x => OnPathComplete(firstEscape, job, x));

    }
    /// <summary>
    /// Update navigation Graph with colliders (from new blocks)
    /// </summary>
    /// <param name="blockColliders"></param>
    private void UpdateGraph(List<Collider2D> blockColliders)
    {
        if (!allowUpdates)
            return;

        foreach(Collider2D collider in blockColliders)
        {
            Bounds bounds = collider.bounds;
            AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => { AstarPath.active.UpdateGraphs(bounds); }));
        }

        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => { UpdateTarget(); }));
    }

    private void Stop(GameOverReasons reason) => Stop();
    
    private void Stop()
    {
        if (seeker != null) seeker.CancelCurrentPathRequest();
        if (ai != null)
        {
            ai.destination = ai.position;
            ai.isStopped = true;
            ((AIPath)ai).enabled = false;
        }
    }
    
    /// <summary>
    /// AI Path Calculation callback
    /// </summary>
    /// <param name="escape"></param>
    /// <param name="job"></param>
    /// <param name="p"></param>
    private void OnPathComplete(Escape escape, Job job, Path p)
    {
        float totalLength = Mathf.Infinity;

        if (p.error)
        {
            if (LogDebug) Debug.LogWarning(p.errorLog);
        }
        else if (Vector3.Distance(((ABPath)p).originalEndPoint, ((ABPath)p).endPoint) > 0.01f)
        {
            if (LogDebug) Debug.LogWarning(string.Format("Path {0} to escape not walkable, job ID : {1}", p.pathID, job.JobId));
        }
        else
        {
            totalLength = p.GetTotalLength();
        }

        // Get Next Job Calc and remove it from processing list
        Escape nextEscape = job.PullRemainingCalc;
        // If new calc, launch new path calculation
        if (nextEscape != null)
            seeker.StartPath(transform.position, nextEscape.gameObject.transform.position, x => OnPathComplete(nextEscape, job, x));

        // Register Result in job
        job.RegisterResult(escape, totalLength);
    }
    /// <summary>
    /// Cleaning - Remove Jobs from runingJobs after job is completed
    /// </summary>
    /// <param name="job"></param>
    private void removeFromJobs(Job job)
    {
        runingJobs.Remove(job);
    }

    private void updateDestination(Job job)
    {
        // If there are no escapes or if all escapes are blocked
        if (job.ShortestEscape == null)
        {
            if (LogDebug) Debug.LogWarning("Error : No Shortest Escape found after calculating path");
            // Stop AI
            ai.isStopped = true;
            // Throw Lose
            GameEvents.TriggerGameOver(GameOverReasons.NoExit);
            return;
        } 
        if(job.ShortestDistance == Mathf.Infinity)
        {
            if (LogDebug) Debug.LogWarning("Error : Shortest Escape found is at Infinity range");
            return;
        }

        Vector3 target = job.ShortestEscape.gameObject.transform.position;
        ai.destination = target;
        seeker.StartPath(transform.position, target, x => OnPathComplete(job.ShortestEscape, job, x));
        // if (ai != null) ai.onSearchPath += UpdateTargetInMovement;
    }
    

    
}