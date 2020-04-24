////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
////using Microsoft.SPOT;
////using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.UI;

namespace GHI.Glide.Display
{
    /// <summary>
    /// The DisplayObjectContainer class is the base class for all objects that can serve as display object containers on the display list.
    /// </summary>
    public class DisplayObjectContainer : DisplayObject
    {
        private ArrayList _displayList = new ArrayList();
        private bool _pressed = false;

        /// <summary>
        /// Renders all children onto this container's graphics.
        /// </summary>
        public override void Render()
        {
            DisplayObject child;
            for (int i = 0; i < _displayList.Count; i++)
            {
                child = (DisplayObject)_displayList[i];
                if (child.Visible)
                    child.Render();
            }
        }

        /// <summary>
        /// Passes touch down events to the children.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        /// <remarks>Events are passed to the children in descending order (from the last child added to the first).</remarks>
        public override TouchEventArgs OnTouchDown(TouchEventArgs e)
        {
            DisplayObject child;
            for (int i = _displayList.Count - 1; i >= 0; i--)
            {
                if (!e.Propagate) break;

                child = (DisplayObject)_displayList[i];

                if (child.Enabled && child.Visible && child.Interactive)
                    e = child.OnTouchDown(e);
            }

            // If nothing was touched, do this Window.
            if (e.Propagate)
                _pressed = true;

            return e;
        }

        /// <summary>
        /// Passes touch up events to the children.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        /// <remarks>Events are passed to the children in descending order (from the last child added to the first).</remarks>
        public override TouchEventArgs OnTouchUp(TouchEventArgs e)
        {
            DisplayObject child;
            for (int i = _displayList.Count - 1; i >= 0; i--)
            {
                if (!e.Propagate) break;

                child = (DisplayObject)_displayList[i];

                if (child.Enabled && child.Visible && child.Interactive)
                    e = child.OnTouchUp(e);
            }

            if (e.Propagate && _pressed)
                TriggerTapEvent(this);

            _pressed = false;

            return e;
        }

        /// <summary>
        /// Passes touch move events to the children.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        /// <remarks>Events are passed to the children in descending order (from the last child added to the first).</remarks>
        public override TouchEventArgs OnTouchMove(TouchEventArgs e)
        {
            DisplayObject child;
            for (int i = _displayList.Count - 1; i >= 0; i--)
            {
                if (!e.Propagate) break;

                child = (DisplayObject)_displayList[i];

                if (child.Enabled && child.Visible && child.Interactive)
                    e = child.OnTouchMove(e);
            }
            return e;
        }

        /// <summary>
        /// Passes touch gesture events to the children.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        /// <remarks>Events are passed to the children in descending order (from the last child added to the first).</remarks>
        public override TouchGestureEventArgs OnTouchGesture(TouchGestureEventArgs e)
        {
            DisplayObject child;
            for (int i = _displayList.Count - 1; i >= 0; i--)
            {
                if (!e.Propagate) break;

                child = (DisplayObject)_displayList[i];

                if (child.Enabled && child.Visible && child.Interactive)
                    e = child.OnTouchGesture(e);
            }

            if (e.Propagate)
                TriggerGestureEvent(this, e);

            return e;
        }

        /// <summary>
        /// Disposes this container and all it's children.
        /// </summary>
        public override void Dispose()
        {
            foreach (var child in _displayList)
                ((DisplayObject)child).Dispose();
            _displayList.Clear();

            Graphics.Dispose();
        }

        /// <summary>
        /// Returns whether this container contains a child DisplayObject.
        /// </summary>
        /// <param name="child">DisplayObject to find.</param>
        /// <returns></returns>
        public bool Contains(DisplayObject child)
        {
            return _displayList.Contains(child);
        }

        /// <summary>
        /// Adds a child DisplayObject to this DisplayObjectContainer.
        /// </summary>
        /// <param name="child">DisplayObject to add.</param>
        public void AddChild(DisplayObject child)
        {
            AddChildAt(-1, child);
        }

        /// <summary>
        /// Adds a child DisplayObject to this DisplayObjectContainer at a specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="child"></param>
        public void AddChildAt(int index, DisplayObject child)
        {
            if (child.Name == null)
                child.Name = "instance" + _displayList.Count;

            if (!_displayList.Contains(child))
            {
                child.Parent = this;

                if (AutoHeight)
                {
                    int maxY = child.Y + child.Height;
                    if (maxY > Height)
                        Height = maxY;
                }

                // Pass the reference to child display object containers
                if (child is DisplayObjectContainer)
                    ((DisplayObjectContainer)child).Graphics = Graphics;

                if (index == -1 || index > _displayList.Count - 1)
                    _displayList.Add(child);
                else
                    _displayList.Insert(index, child);
            }
        }

        /// <summary>
        /// Removes the specified child DisplayObject from this DisplayObjectContainer.
        /// </summary>
        /// <param name="child">DisplayObject to remove.</param>
        public void RemoveChild(DisplayObject child)
        {
            if (_displayList.Contains(child))
                _displayList.Remove(child);
        }

        /// <summary>
        /// Removes the specified child DisplayObject from this DisplayObjectContainer at a specific index.
        /// </summary>
        /// <param name="index">Index of a DisplayObject.</param>
        public void RemoveChildAt(int index)
        {
            if (_displayList.Count > 0 && index > -1 && index < _displayList.Count)
                _displayList.RemoveAt(index);
        }

        /// <summary>
        /// Returns a DisplayObject with the specified name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        public DisplayObject GetChildByName(string name)
        {
            for (int i = 0; i < _displayList.Count; i++)
            {
                if (((DisplayObject)_displayList[i]).Name == name)
                    return (DisplayObject)_displayList[i];
            }
            return null;
        }

        /// <summary>
        /// Returns a child DisplayObject by index.
        /// </summary>
        /// <param name="index">Index of a DisplayObject.</param>
        /// <returns></returns>
        public virtual DisplayObject this[int index]
        {
            get { return (DisplayObject)_displayList[index]; }
        }

        /// <summary>
        /// Graphics specifies an object that belongs to this container where drawing commands can occur.
        /// </summary>
        public Graphics Graphics { get; set; }

        /// <summary>
        /// Returns the number of children of this object.
        /// </summary>
        public int NumChildren
        {
            get { return _displayList.Count; }
        }

        /// <summary>
        /// Indicates whether or not the container should automatically resize it's height.
        /// </summary>
        public bool AutoHeight { get; set; }
    }
}
