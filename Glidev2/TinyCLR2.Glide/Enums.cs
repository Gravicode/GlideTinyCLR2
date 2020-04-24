////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
////using Microsoft.SPOT;

namespace GHI.Glide
{
    /// <summary>
    /// Direction
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Left
        /// </summary>
        Left,

        /// <summary>
        /// Right
        /// </summary>
        Right,

        /// <summary>
        /// Up
        /// </summary>
        Up,

        /// <summary>
        ///  Down
        /// </summary>
        Down
    }

    /// <summary>
    /// Horizontal Alignment
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Left
        /// </summary>
        Left,

        /// <summary>
        /// Center
        /// </summary>
        Center,

        /// <summary>
        /// Right
        /// </summary>
        Right
    }

    /// <summary>
    /// Vertical Alignment
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// Top
        /// </summary>
        Top,

        /// <summary>
        /// Middle
        /// </summary>
        Middle,

        /// <summary>
        /// Bottom
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Modal Buttons
    /// </summary>
    public enum ModalButtons
    {
        /// <summary>
        /// Ok
        /// </summary>
        Ok,

        /// <summary>
        /// Ok, Cancel
        /// </summary>
        OkCancel,

        /// <summary>
        /// Abort, Retry, Ignore
        /// </summary>
        AbortRetryIgnore,

        /// <summary>
        /// Yes, No
        /// </summary>
        YesNo,

        /// <summary>
        /// Yes, No, Cancel
        /// </summary>
        YesNoCancel,

        /// <summary>
        /// Retry, Cancel
        /// </summary>
        RetryCancel
    }

    /// <summary>
    /// Modal Result
    /// </summary>
    public enum ModalResult
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Ok
        /// </summary>
        Ok,

        /// <summary>
        /// Cancel
        /// </summary>
        Cancel,

        /// <summary>
        /// Abort
        /// </summary>
        Abort,

        /// <summary>
        /// Retry
        /// </summary>
        Retry,

        /// <summary>
        /// Ignore
        /// </summary>
        Ignore,

        /// <summary>
        /// Yes
        /// </summary>
        Yes,

        /// <summary>
        /// No
        /// </summary>
        No
    }
}
