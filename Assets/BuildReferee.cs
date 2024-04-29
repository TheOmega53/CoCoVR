using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.OpenXR.Input;


public class BuildReferee : MonoBehaviour
{
    public List<Collider> colliders = new List<Collider>();

    public Dictionary<Collider, bool> GoalBlocks = new Dictionary<Collider,bool>();
    
    public int BlocksFilled;

    public UnityEvent OnAccomplished;


    private void Start()
    {
           foreach (var collider in colliders)
        {
            GoalBlocks.Add(collider, false);
        }
    }


    private void Update()
    {
        if (CheckIfCorrect())
        {
            OnAccomplished?.Invoke();
        }
    }

    private bool CheckIfCorrect()
    {
        BlocksFilled = 0;
        foreach (KeyValuePair<Collider, bool> block in GoalBlocks)
        {
            if (block.Value == true)
            {
                BlocksFilled++;
            }
        }

        Debug.Log(BlocksFilled + " Out of " + GoalBlocks.Count + " Blocks filled");

        if(BlocksFilled == GoalBlocks.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void BlockFilled(Collider collider)
    {
        GoalBlocks[collider] = true;
    }

    internal void BlockEmptied(Collider collider)
    {
        GoalBlocks[collider] = false;
    }
}
