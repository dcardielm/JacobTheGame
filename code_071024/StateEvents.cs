using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace Corndog
{
    // Container for event subscriptions, Will help organize flow of interactable and other
    // classes. Will work much the same as states, but be entirely event driven.
    public abstract class EventState<T>
    {
        protected T caller; 

        public EventState(T caller)
        {
            this.caller = caller;
        }

        public abstract void Enter();

        public abstract void Exit();
    }
}
