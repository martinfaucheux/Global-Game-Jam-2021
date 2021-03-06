using System;

using UnityEditor;
using UnityEngine;

using Codice.AssetsProcessor;
using Codice.CM.Common;
using Codice.Tool;
using Codice.UI;
using Codice.Views.Changesets;
using Codice.Views.IncomingChanges;
using Codice.Views.PendingChanges;
using GluonGui;
using PlasticGui;
using PlasticGui.Gluon;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Merge;

using GluonNewIncomingChangesUpdater = PlasticGui.Gluon.WorkspaceWindow.NewIncomingChangesUpdater;

namespace Codice
{
    public class ViewSwitcher :
        IViewSwitcher, IMergeViewLauncher, IGluonViewSwitcher
    {
        public PendingChangesTab PendingChangesTabForTesting { get { return mPendingChangesTab; } }
        public IIncomingChangesTab IncomingChangesTabForTesting { get { return mIncomingChangesTab; } }

        internal ViewSwitcher(
            WorkspaceInfo wkInfo,
            ViewHost viewHost,
            bool isGluonMode,
            PlasticGui.WorkspaceWindow.PendingChanges.PendingChanges pendingChanges,
            NewIncomingChangesUpdater developerNewIncomingChangesUpdater,
            GluonNewIncomingChangesUpdater gluonNewIncomingChangesUpdater,
            IIncomingChangesNotificationPanel incomingChangesNotificationPanel,
            EditorWindow parentWindow)
        {
            mWkInfo = wkInfo;
            mViewHost = viewHost;
            mIsGluonMode = isGluonMode;
            mPendingChanges = pendingChanges;
            mDeveloperNewIncomingChangesUpdater = developerNewIncomingChangesUpdater;
            mGluonNewIncomingChangesUpdater = gluonNewIncomingChangesUpdater;
            mIncomingChangesNotificationPanel = incomingChangesNotificationPanel;
            mParentWindow = parentWindow;
        }

        internal void SetPlasticGUIClient(PlasticGUIClient plasticClient)
        {
            mPlasticClient = plasticClient;
        }

        internal void ShowInitialView()
        {
            ShowPendingChangesView();
        }

        internal void AutoRefreshPendingChangesView()
        {
            AutoRefresh.PendingChangesView(
                mPendingChangesTab);
        }

        internal void AutoRefreshIncomingChangesView()
        {
            AutoRefresh.IncomingChangesView(
                mIncomingChangesTab);
        }

        internal void OnDisable()
        {
            PlasticAssetsProcessor.UnRegisterViews();

            if (mPendingChangesTab != null)
                mPendingChangesTab.OnDisable();

            if (mIncomingChangesTab != null)
                mIncomingChangesTab.OnDisable();

            if (mChangesetsTab != null)
                mChangesetsTab.OnDisable();
        }

        internal void Update()
        {
            if (IsViewSelected(SelectedTab.PendingChanges))
            {
                mPendingChangesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.IncomingChanges))
            {
                mIncomingChangesTab.Update();
                return;
            }

            if (IsViewSelected(SelectedTab.Changesets))
            {
                mChangesetsTab.Update();
                return;
            }
        }

        internal void TabButtonsGUI()
        {
            InitializeTabButtonWidth();

            bool wasPendingChangesSelected =
                IsViewSelected(SelectedTab.PendingChanges);
            bool isPendingChangesSelected =
                DoTabButton(PENDING_CHANGES_TEXT,
                    mTabButtonWidth, wasPendingChangesSelected);

            if (isPendingChangesSelected)
                ShowPendingChangesView();

            bool wasIncomingChangesSelected =
                IsViewSelected(SelectedTab.IncomingChanges);
            bool isIncomingChangesSelected =
                DoTabButton(INCOMING_CHANGES_BUTTON_TEXT,
                    mTabButtonWidth, wasIncomingChangesSelected);

            if (isIncomingChangesSelected)
                ShowIncomingChangesView();

            bool wasChangesetsSelected =
                IsViewSelected(SelectedTab.Changesets);
            bool isChangesetsSelected =
                DoTabButton(CHANGESETS_BUTTON_TEXT,
                    mTabButtonWidth, wasChangesetsSelected);

            if (isChangesetsSelected)
                ShowChangesetsView();
        }

        internal void TabViewGUI()
        {
            if (IsViewSelected(SelectedTab.PendingChanges))
            {
                mPendingChangesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.IncomingChanges))
            {
                mIncomingChangesTab.OnGUI();
                return;
            }

            if (IsViewSelected(SelectedTab.Changesets))
            {
                mChangesetsTab.OnGUI();
                return;
            }
        }

        void IViewSwitcher.ShowPendingChanges()
        {
            ShowPendingChangesView();
        }

        void IViewSwitcher.ShowSyncView(string syncViewToSelect)
        {
            throw new NotImplementedException();
        }

        void IViewSwitcher.ShowBranchExplorerView()
        {
            //TODO: Codice
            //launch plastic with branch explorer view option
        }

        void IViewSwitcher.DisableMergeView()
        {
        }

        bool IViewSwitcher.IsIncomingChangesView()
        {
            return IsViewSelected(SelectedTab.IncomingChanges);
        }

        void IViewSwitcher.CloseIncomingChangesView()
        {
            ((IViewSwitcher)this).DisableMergeView();
        }

        void IMergeViewLauncher.MergeFrom(ObjectInfo objectInfo, EnumMergeType mergeType)
        {
            ((IMergeViewLauncher)this).MergeFromInterval(objectInfo, null, mergeType);
        }

        void IMergeViewLauncher.MergeFrom(ObjectInfo objectInfo, EnumMergeType mergeType, ShowIncomingChangesFrom from)
        {
            ((IMergeViewLauncher)this).MergeFromInterval(objectInfo, null, mergeType);
        }

        void IMergeViewLauncher.MergeFromInterval(ObjectInfo objectInfo, ObjectInfo ancestorChangesetInfo, EnumMergeType mergeType)
        {
            if (mergeType == EnumMergeType.IncomingMerge)
            {
                ShowIncomingChangesView();
                mParentWindow.Repaint();
                return;
            }

            LaunchTool.OpenMerge(mWkInfo.ClientPath);
        }

        void IGluonViewSwitcher.ShowIncomingChangesView()
        {
            ShowIncomingChangesView();
            mParentWindow.Repaint();
        }

        void ShowPendingChangesView()
        {
            if (mPendingChangesTab == null)
            {
                mPendingChangesTab = new PendingChangesTab(
                    mWkInfo,
                    mPlasticClient,
                    mIsGluonMode,
                    mPendingChanges,
                    mDeveloperNewIncomingChangesUpdater,
                    mParentWindow);

                mViewHost.AddRefreshableView(
                    ViewType.CheckinView,
                    mPendingChangesTab);

                PlasticAssetsProcessor.
                    RegisterPendingChangesView(mPendingChangesTab);
            }

            bool wasPendingChangesSelected =
                IsViewSelected(SelectedTab.PendingChanges);

            if (!wasPendingChangesSelected)
            {
                mPendingChangesTab.AutoRefresh();
            }

            SetSelectedView(SelectedTab.PendingChanges);
        }

        void ShowIncomingChangesView()
        {
            if (mIncomingChangesTab == null)
            {
                mIncomingChangesTab = mIsGluonMode ?
                    new Views.IncomingChanges.Gluon.IncomingChangesTab(
                        mWkInfo,
                        mViewHost,
                        mPlasticClient,
                        mGluonNewIncomingChangesUpdater,
                        (Gluon.IncomingChangesNotificationPanel)mIncomingChangesNotificationPanel,
                        mParentWindow) as IIncomingChangesTab :
                    new Views.IncomingChanges.Developer.IncomingChangesTab(
                        mWkInfo,
                        this,
                        mPlasticClient,
                        mDeveloperNewIncomingChangesUpdater,
                        mParentWindow);

                mViewHost.AddRefreshableView(
                    ViewType.IncomingChangesView,
                    (IRefreshableView)mIncomingChangesTab);

                PlasticAssetsProcessor.
                    RegisterIncomingChangesView(mIncomingChangesTab);
            }

            bool wasIncomingChangesSelected =
                IsViewSelected(SelectedTab.IncomingChanges);

            if (!wasIncomingChangesSelected)
                mIncomingChangesTab.AutoRefresh();

            SetSelectedView(SelectedTab.IncomingChanges);
        }

        void ShowChangesetsView()
        {
            if (mChangesetsTab == null)
            {
                mChangesetsTab = new ChangesetsTab(
                    mWkInfo,
                    mPlasticClient,
                    this,
                    mParentWindow,
                    mIsGluonMode);

                mViewHost.AddRefreshableView(
                    ViewType.ChangesetsView,
                    mChangesetsTab);
            }

            bool wasChangesetsSelected =
                IsViewSelected(SelectedTab.Changesets);

            if (!wasChangesetsSelected)
                ((IRefreshableView)mChangesetsTab).Refresh();

            SetSelectedView(SelectedTab.Changesets);
        }

        bool IsViewSelected(SelectedTab tab)
        {
            return mSelectedTab == tab;
        }

        void SetSelectedView(SelectedTab tab)
        {
            mSelectedTab = tab;

            if (mIncomingChangesTab == null)
                return;

            mIncomingChangesTab.IsVisible =
                tab == SelectedTab.IncomingChanges;
        }

        static bool DoTabButton(
            string buttonText, float tabButtonWidth, bool wasActive)
        {
            GUIContent buttonContent = new GUIContent(buttonText);

            GUIStyle buttonStyle = (wasActive) ?
               UnityStyles.PlasticWindow.ActiveTabButton :
               EditorStyles.toolbarButton;

            Rect toggleRect = GUILayoutUtility.GetRect(
                buttonContent, buttonStyle,
                GUILayout.Width(tabButtonWidth));

            bool isActive = GUI.Toggle(
                toggleRect, wasActive, buttonText, buttonStyle);

            if (wasActive)
            {
                GUIStyle activeTabStyle =
                    UnityStyles.PlasticWindow.ActiveTabUnderline;

                Rect underlineRect = new Rect(
                    toggleRect.x,
                    toggleRect.yMax - activeTabStyle.fixedHeight,
                    toggleRect.width,
                    activeTabStyle.fixedHeight);

                GUI.Label(underlineRect, string.Empty, activeTabStyle);
            }

            return isActive;
        }

        void InitializeTabButtonWidth()
        {
            if (mTabButtonWidth != -1)
                return;

            mTabButtonWidth = MeasureMaxWidth.ForTexts(
                UnityStyles.PlasticWindow.ActiveTabButton,
                PENDING_CHANGES_TEXT,
                INCOMING_CHANGES_BUTTON_TEXT,
                CHANGESETS_BUTTON_TEXT);
        }

        enum SelectedTab
        {
            None = 0,
            PendingChanges = 1,
            IncomingChanges = 2,
            Changesets = 3
        }

        PendingChangesTab mPendingChangesTab;
        IIncomingChangesTab mIncomingChangesTab;
        ChangesetsTab mChangesetsTab;

        float mTabButtonWidth = -1;
        SelectedTab mSelectedTab;

        PlasticGUIClient mPlasticClient;

        readonly EditorWindow mParentWindow;
        readonly IIncomingChangesNotificationPanel mIncomingChangesNotificationPanel;
        readonly GluonNewIncomingChangesUpdater mGluonNewIncomingChangesUpdater;
        readonly NewIncomingChangesUpdater mDeveloperNewIncomingChangesUpdater;
        readonly PlasticGui.WorkspaceWindow.PendingChanges.PendingChanges mPendingChanges;
        readonly bool mIsGluonMode;
        readonly ViewHost mViewHost;
        readonly WorkspaceInfo mWkInfo;

        const string PENDING_CHANGES_TEXT = "Pending changes";
        const string INCOMING_CHANGES_BUTTON_TEXT = "Incoming changes";
        const string CHANGESETS_BUTTON_TEXT = "Changesets";
    }
}
