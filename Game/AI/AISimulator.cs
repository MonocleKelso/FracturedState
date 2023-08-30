using FracturedState.Game.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISimulator : MonoBehaviour
{
    public static AISimulator Instance { get; private set; }

    private const float simulateTime = 15;

    private List<Team> aiTeams;
    private bool simulationRunning;
    private Coroutine sim;

    void Awake()
    {
        Instance = this;
        aiTeams = new List<Team>();
    }

    public void AddTeam(Team team)
    {
        if (!aiTeams.Contains(team))
        {
            aiTeams.Add(team);
        }
    }

    public void RemoveTeam(Team team)
    {
        if (!simulationRunning)
        {
            aiTeams.Remove(team);
        }
    }

    public void ClearTeams()
    {
        if (!simulationRunning)
        {
            aiTeams.Clear();
        }
    }

    public void StartSimulation()
    {
        sim = StartCoroutine(Simulate());
    }

    public void StopSimulation()
    {
        if (sim != null)
        {
            StopCoroutine(sim);
            sim = null;
            simulationRunning = false;
            foreach (var aiTeam in aiTeams)
            {
                aiTeam.AIBrain.Terminate();
            }
        }
    }

    void Update()
    {
        if (simulationRunning)
        {
            for (int i = 0; i < aiTeams.Count; i++)
            {
                if (aiTeams[i].IsActive)
                {
                    aiTeams[i].RecruitManager.UpdateRequestTimes(Time.deltaTime);
                }
            }
        }
    }

    IEnumerator Simulate()
    {
        simulationRunning = true;
        for (int i = 0; i < aiTeams.Count; i++)
        {
            aiTeams[i].AIBrain.Activate();
        }
        yield return new WaitForSeconds(simulateTime);
        while (true)
        {
            for (int i = 0; i < aiTeams.Count; i++)
            {
                if (aiTeams[i].IsActive)
                {
                    aiTeams[i].AIBrain.Process();
                }
                yield return null;
            }
            yield return new WaitForSeconds(simulateTime);
        }
    }
}