using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

// Common place for classes that contain on Actions that other
// classes can hook into.
namespace Corndog.GameplayTriggers
{
    public class CharacterTriggers
    {
        public delegate void InputTrigger();

        public Action OnFire;
        public Action OnReload;
        public Action OnStore;
        public Action OnDrop;
        public Action<Interactables.InteractableObject> OnToStorage;

        public delegate void SecondaryTrigger();


        public Action<GameObject> OnPickUp;
    }


    public class GameplayTriggers
    {

        public Action OnFire;
        public Action OnReload;
        public Action OnStore;
        public Action OnDrop;
    }

    public class ProcessTriggers
    {   
        CharacterTriggers c;
        Listener l1;
        Listener l2;

        Action filler;

        public void Test()
        {
            l1 = new Listener(ref c.OnFire, PlayFireSound, ref c.OnStore);
            l1 += filler;
            filler += l1;
            l1 += l2;
        }

        void PlayFireSound()
        {
            // pew pew
        }
    }

    public struct Listener
    {  
        private Action trigger;
        private Action actions;
        public Listener(ref Action onAction, Action doAction, ref Action killFlag)
        {
            trigger = onAction;
            actions = doAction;
            killFlag += HandleKill;
        }

        private void HandleKill() => actions = null;

        public static implicit operator Action(Listener l) => l.actions;
        public static implicit operator Listener(Action a) => a;

        public static Listener operator +(Listener l1, Listener l2) => l1.actions += l2.actions;
        public static Listener operator -(Listener l1, Listener l2) => l1.actions -= l2.actions;
    }
}

//     public struct Test
//     {
//         int _h;
//         Type _t;

//         public Test(object @object)
//         {
//             _h = @object.GetHashCode();
//             _t = @object.GetType();
//         }

//         public delegate void Ter();
//         public Action fff;

//         public static bool operator ==(Test t1, Test t2)
//         {
//             return t1._h == t2._h && t1._t == t2._t;
//         }

//         public static bool operator !=(Test t1, Test t2)
//         {
//             return t1._h != t2._h || t1._t != t2._t;
//         }

//         public static implicit operator Action(Test t) => t.fff;
//     }

//     public class RefArray : IEnumerable
//     {
//         private Test[] _t;
        
//         public RefArray(Test[] tests)
//         {
//             new Test(tests);
//         }

//         IEnumerator IEnumerable.GetEnumerator()
//         {
//             return (IEnumerator) GetEnumerator();
//         }

//         public RefEnumerator GetEnumerator()
//         {
//             return new RefEnumerator(_t);
//         }



//         public struct RefEnumerator : IEnumerator
//         {
//             public Test[] _t;
//             int index;
//             public RefEnumerator(Test[] tests)
//             {
//                 _t = tests;
//                 index = -1;
//             }

//             public bool MoveNext()
//             {
//                 return false;
//             }

//             public void Reset()
//             {

//             }

//             object IEnumerator.Current
//             {
//                 get { return Current; }
//             }

//             public Test Current
//             {
//                 get
//                 {
//                     try
//                     {
//                         return _t[index];
//                     }
//                     catch (IndexOutOfRangeException)
//                     {
//                         throw new InvalidOperationException();
//                     }
//                 }
//             }
//         }
//     }
// }
