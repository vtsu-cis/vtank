/*!
    \file   StateManager.cs
    \brief  Keep track of the current VTank state.
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework;

namespace Client.src.service.services
{
    /// <summary>
    /// The StateManager tracks VTank's current state.  The game will draw and interact with the user
    /// according to which state it is in.  The advantage of using a stack for tracking states is that
    /// one could load multiple states into the stack.  XNA will draw the states according to stack location;
    /// the top of the stack is drawn first, and the bottom last.
    /// </summary>
    public sealed class StateManager
    {
        private State currentState;

        /// <summary>
        /// Initialize the state manager.
        /// </summary>
        /// <param name="game">Used for adding or removing game components.</param>
        public StateManager()
        {
            currentState = new LoginScreenState();
        }

        /// <summary>
        /// Remove all existing states and set it to the given new state.
        /// </summary>
        /// <param name="newState">State to set the game to.</param>
        public void ChangeState(State newState)
        {
            State oldState = currentState;

            currentState = newState;

            oldState.UnloadContent();
            oldState = null;

            currentState.Initialize();
            currentState.LoadContent();
        }

        /// <summary>
        /// Conveniently allows the user to either peek at the top-most (current) state on the stack or
        /// set the state to something else.
        /// </summary>
        public State CurrentState
        {
            get { return currentState; }
        }

        /// <summary>
        /// Changes to a new state.
        /// </summary>
        /// <typeparam name="T">The state to change to</typeparam>
        public void ChangeState<T>() where T : State
        {
            ChangeState((T)Activator.CreateInstance(typeof(T), true));
        }
    }
}