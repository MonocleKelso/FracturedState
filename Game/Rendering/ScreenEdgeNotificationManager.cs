using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game;
using FracturedState.Game.Data;
using System;

namespace FracturedState.UI
{
    public sealed class ScreenEdgeNotificationManager
    {
        private static ScreenEdgeNotificationManager instance;
        public static ScreenEdgeNotificationManager Instance => instance ?? (instance = new ScreenEdgeNotificationManager());

        private const string CanvasName = "ScreenEdgeCanvas";
        private readonly GameObject canvas;
        private readonly CompassMenuManager compass;
        private Dictionary<Squad, RectTransform> activeBattleIcons = new Dictionary<Squad, RectTransform>();
        private Dictionary<Squad, RectTransform> activeRecruitIcons = new Dictionary<Squad, RectTransform>();
        private Dictionary<RectTransform, float> recruitTimers = new Dictionary<RectTransform, float>();
        private Squad[] battleSquads;
        private Squad[] recruitSquads;

        private Action<Squad, RectTransform> battleAction = (s, i) =>
        {
            if (s.Members.Count == 0)
            {
                instance.RemoveBattleNotification(s);
            }
            else
            {
                i.gameObject.SetActive(false);
            }
        };
        private readonly Action<Squad, RectTransform> recruitAction = (s, i) => { instance.RemoveRecruitNotification(s); };

        private readonly float width;
        private readonly float height;

        private bool doUpdate;

        public static void Init()
        {
            if (instance == null)
            {
                instance = new ScreenEdgeNotificationManager();
            }
        }

        private ScreenEdgeNotificationManager()
        {
            canvas = GameObject.Find(CanvasName);
            compass = UnityEngine.Object.FindObjectOfType<CompassMenuManager>();
            width = Screen.width;
            height = Screen.height;
            doUpdate = true;
        }

        public void SetUpdate(bool update)
        {
            doUpdate = update;
            var bIcons = activeBattleIcons.Values.ToList();
            for (var i = 0; i < bIcons.Count; i++)
            {
                if (bIcons[i] != null)
                {
                    bIcons[i].gameObject.SetActive(doUpdate);
                }
            }
            var rIcons = activeRecruitIcons.Values.ToList();
            for (var i = 0; i < rIcons.Count; i++)
            {
                if (rIcons[i] != null)
                {
                    rIcons[i].gameObject.SetActive(doUpdate);
                }
            }
        }

        public void UpdateBattleIcons()
        {
            if (!doUpdate)
                return;

            if (battleSquads != null)
            {
                for (var i = 0; i < battleSquads.Length; i++)
                {
                    var squad = battleSquads[i];
                    var icon = activeBattleIcons[squad];
                    AdjustIconPosition(icon, squad, battleAction);
                }
            }

            if (recruitSquads != null)
            {
                var c = new Squad[recruitSquads.Length];
                recruitSquads.CopyTo(c, 0);
                for (var i = 0; i < c.Length; i++)
                {
                    var squad = c[i];
                    var icon = activeRecruitIcons[squad];
                    AdjustIconPosition(icon, squad, recruitAction);
                    float time;
                    if (recruitTimers.TryGetValue(icon, out time))
                    {
                        if (Time.time - time > 60)
                        {
                            RemoveRecruitNotification(squad);
                        }
                        else if (Time.time - time > 45)
                        {
                            icon.GetComponent<Animator>().enabled = true;
                        }
                    }
                }
            }
        }

        void AdjustIconPosition(RectTransform icon, Squad squad, Action<Squad, RectTransform> onscreenAction)
        {
            var worldPos = squad.GetAveragePosition();
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            // negative z means position is 'behind' camera which gives weird results
            // for screen position - therefore we invert x and y to keep indicators from jumping to
            // top of screen incorrectly
            if (screenPos.z < 0)
            {
                screenPos.x = -screenPos.x;
                screenPos.y = -screenPos.y;
            }
            if ((screenPos.x < 0 || screenPos.x > width) || (screenPos.y < 0 || screenPos.y > height))
            {
                if (!icon.gameObject.activeSelf)
                    icon.gameObject.SetActive(true);

                var x = Mathf.Clamp(screenPos.x, 0, width);
                var y = Mathf.Clamp(screenPos.y, 0, height);

                // make sure indicator doesn't bleed into compass
                if (x > compass.CompassRect.xMin && y > compass.CompassRect.yMin)
                {
                    // if we're attached to the top of the screen then clamp x
                    if (Mathf.Approximately(y, height))
                    {
                        x = compass.CompassRect.xMin;
                    }
                    // otherwise clamp y
                    else
                    {
                        y = compass.CompassRect.yMin;
                    }
                }
                // if the recruit panel is open then make sure indicator avoids it as well
                if (compass.UI.RecruitPanel)
                {
                    if (x > compass.UI.RecruitArea.xMin && y > compass.UI.RecruitArea.yMin && !Mathf.Approximately(y, height))
                    {

                        x = width;
                        y = compass.UI.RecruitArea.yMin;
                    }
                }

                // avoid game clock and territory read out when we're at the top of the screen
                if (Mathf.Approximately(y, height))
                {
                    if (x < compass.UI.Timers.GameTimeRect.xMax)
                    {
                        y = height - compass.UI.Timers.GameTimeRect.yMax;
                    }
                    else if (x > compass.UI.Timers.TerritoryReadOutRect.xMin && x < compass.UI.Timers.TerritoryReadOutRect.xMax)
                    {
                        y = height - compass.UI.Timers.TerritoryReadOutRect.yMax;
                    }
                }

                icon.position = new Vector3(x, y, 0);

                var r = Quaternion.LookRotation(icon.position - screenPos, Vector3.forward);
                r.x = 0;
                r.y = 0;
                icon.rotation = r;
            }
            else
            {
                onscreenAction?.Invoke(squad, icon);
            }
        }

        public void RequestRecruitNotification(Squad squad)
        {
            var recruitIcon = DataUtil.LoadPrefab(DataLocationConstants.RecruitIconPrefab);
            recruitIcon.transform.SetParent(canvas.transform, false);
            recruitIcon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => MoveToSquad(squad, squad.GetAveragePosition()));
            var r = recruitIcon.GetComponent<RectTransform>();
            activeRecruitIcons[squad] = r;
            recruitTimers[r] = Time.time;
            recruitSquads = new Squad[activeRecruitIcons.Keys.Count];
            activeRecruitIcons.Keys.CopyTo(recruitSquads, 0);
            if (!doUpdate)
            {
                recruitIcon.SetActive(false);
            }
        }

        public void RemoveRecruitNotification(Squad squad)
        {
            RectTransform icon;
            if (activeRecruitIcons.TryGetValue(squad, out icon))
            {
                recruitTimers.Remove(icon);
                UnityEngine.Object.Destroy(icon.gameObject);
                activeRecruitIcons.Remove(squad);
                recruitSquads = new Squad[activeRecruitIcons.Keys.Count];
                activeRecruitIcons.Keys.CopyTo(recruitSquads, 0);
            }
        }

        public void RequestBattleNotification(Squad squad)
        {
            if (!activeBattleIcons.ContainsKey(squad))
            {
                var battleIcon = DataUtil.LoadPrefab(DataLocationConstants.BattleIconPrefab);
                battleIcon.transform.SetParent(canvas.transform, false);
                battleIcon.GetComponent<ScreenEdgeIconController>().Init(squad);
                battleIcon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => MoveToSquad(squad, squad.GetAveragePosition()));
                activeBattleIcons[squad] = battleIcon.GetComponent<RectTransform>();
                battleSquads = new Squad[activeBattleIcons.Keys.Count];
                activeBattleIcons.Keys.CopyTo(battleSquads, 0);
                if (!doUpdate)
                {
                    battleIcon.SetActive(false);
                }
            }
            else
            {
                var icon = activeBattleIcons[squad];
                // calling init again stops the fade coroutine if it's running
                icon.GetComponent<ScreenEdgeIconController>().Init(squad);
            }
        }

        public void RemoveBattleNotification(Squad squad)
        {
            RectTransform icon;
            if (activeBattleIcons.TryGetValue(squad, out icon))
            {
                if (icon != null)
                {
                    icon.GetComponent<ScreenEdgeIconController>().StartRemoval();
                }
                else
                {
                    activeBattleIcons.Remove(squad);
                }
            }
        }

        public void RemoveIcon(Squad squad)
        {
            if (activeBattleIcons.ContainsKey(squad))
            {
                UnityEngine.Object.Destroy(activeBattleIcons[squad].gameObject);
                activeBattleIcons.Remove(squad);
                battleSquads = new Squad[activeBattleIcons.Keys.Count];
                activeBattleIcons.Keys.CopyTo(battleSquads, 0);
            }
        }

        public void RemoveAllIcons()
        {
            RectTransform rt;
            foreach (var icon in activeBattleIcons.Keys)
            {
                if (activeBattleIcons.TryGetValue(icon, out rt))
                {
                    UnityEngine.Object.Destroy(rt.gameObject);
                }
            }
            foreach (var icon in activeRecruitIcons.Keys)
            {
                if (activeRecruitIcons.TryGetValue(icon, out rt))
                {
                    UnityEngine.Object.Destroy(rt.gameObject);
                }
            }
            activeBattleIcons = new Dictionary<Squad, RectTransform>();
            activeRecruitIcons = new Dictionary<Squad, RectTransform>();
            recruitTimers = new Dictionary<RectTransform, float>();
            battleSquads = null;
            recruitSquads = null;
        }

        private void MoveToSquad(Squad squad, Vector3 location)
        {
            RemoveIcon(squad);
            Camera.main.GetComponent<CommonCameraController>().BattleMove(location);
        }
    }
}