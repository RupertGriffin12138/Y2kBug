using System;
using Save;
using UnityEngine;

namespace Condition
{
    public class GuardCheck : MonoBehaviour
    {
        public GameObject guard;


        public GameObject guardTrigger;

        private void Start()
        {
            if (guard)
            {
                guard.SetActive(false);
            }
            
        }

        private void Update()
        {
            if (guardTrigger&&  guardTrigger.activeSelf && guard)
            {
                guard.SetActive(true);
            }
            else
            {
                guard.SetActive(false);
            }
        }
    }
}