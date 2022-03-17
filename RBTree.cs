using System;
using System.Collections.Generic;
using System.Text;

namespace RedBlackTrees
{
    public class RBTree<T>
    {
        public IComparer<T> Comparer { get; protected set; }

        public TreeNode<T> Root;

        public int Depth { get; protected set; } // black depth;

        public RBTree()
        {
            this.Comparer = Comparer<T>.Default;
            this.Root = null;
            this.Depth = 0;
        }

        public RBTree(IComparer<T> comparer)
        {
            this.Comparer = comparer;
            this.Root = null;
            this.Depth = 0;
        }

        public bool BinarySearchFirst(T item, out TreeNode<T> first, out NodeDirection direction)
            => BinarySearch(item, out first, out direction);
        //public bool BinarySearchLeft(T item, out TreeNode<T> lessThanOrEqual)
        //    => BinarySearch(item, out lessThanOrEqual, out );

        internal bool BinarySearch(T item, out TreeNode<T> output, out NodeDirection lastMove)
        {
            TreeNode<T> cur = this.Root;

            lastMove = NodeDirection.None;
            if (cur == null) { output = null; return false; }

            while (true)
            {
                int cmp = Comparer.Compare(item, cur.Item);
                if (cmp == 0) { output = cur; return true; }
                else if (cmp > 0)
                {
                    if (cur.Right == null)
                    {
                        lastMove = NodeDirection.Right;
                        break;
                    }
                    cur = cur.Right;
                }
                else
                {
                    if (cur.Left == null)
                    {
                        lastMove = NodeDirection.Left;
                        break;
                    }
                    cur = cur.Left;
                }
            }
            output = cur;
            return false;
        }

        public bool Add(T item)
        {
            if (BinarySearchFirst(item, out var first, out var direction))
                return false;
            if (first == null)
            {
                Root = new TreeNode<T>(item) { Color = RbColor.Black };
                Depth++;
                return true;
            }

            var added = new TreeNode<T>(item) { Color = RbColor.Red }; // to make sure

            if (direction == NodeDirection.Left)
                first.AddChildLeft(added);
            else if (direction == NodeDirection.Right)
                first.AddChildRight(added);
            else throw new InvalidOperationException("should have returned true in binary search");

            // Try Recoloring and Shifting
            var child = added;
            while (true) // child will be always red in this loop
            {
                var parent = child.Parent;
                if (parent == null)
                {
                    child.Color = RbColor.Black;
                    Depth++;
                    break;
                }
                if (parent.Color == RbColor.Black) break;
                // now parent color is red, so it cannot be the root
                if (parent.Parent == null) throw new InvalidOperationException("RB Tree violated");

                var uncle = parent.Parent.Left;
                bool parentLeft = false;
                if (uncle == parent)
                {
                    uncle = parent.Parent.Right;
                    parentLeft = true;
                }
                if (!IsBlack(uncle))
                {
                    parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    child = parent.Parent; // proceed to grandparent;
                    child.Color = RbColor.Red;
                    continue;
                }
                // now uncle color = Black,
                // switch into 4 cases accridng to uncleLeft, childLeft
                bool childLeft = parent.Left == child;

                // LL case
                //   g : Black                      p : changed to Black
                //  p:Red u:Black        =>        c:Red  g : changed to Red
                // c:Red x:(Black by assumption)         x:Black u :Black
                if (parentLeft) // case LL
                {
                    //case LR
                    if (!childLeft)
                        parent = parent.ShiftLeft(); // this reduces to case LL
                    //case LL and LR
                    parent = parent.Parent.ShiftRight();
                    if (parent.Parent == null) Root = parent;
                    parent.Color = RbColor.Black;
                    parent.Right.Color = RbColor.Red;
                    break;
                }
                else
                {
                    // case RL
                    if (childLeft)
                        parent = parent.ShiftRight();
                    // case RR and RL
                    parent = parent.Parent.ShiftLeft();
                    if (parent.Parent == null) Root = parent;
                    parent.Color = RbColor.Black;
                    parent.Left.Color = RbColor.Red;
                    break;
                }
            }

            return true;
        }

        public bool Remove(T item)
        {
            if (!BinarySearchFirst(item, out var first, out _))
                return false;
            return Remove(first);
        }

        public bool Remove(TreeNode<T> item)
        {
            var parent = item.Parent; // save parent first;
            var succ = item.RemoveSelf(out var type, out var selfNextParent,
                                       out var selfNextChild);
            if (parent == null)
                Root = succ;

            switch (type)
            {
                case TreeRemovalType.LeftNull:
                case TreeRemovalType.RightNull:

                    if (item.Color == RbColor.Black)
                        RemoveFixup(succ, parent); // in case succ is null
                    break;
                case TreeRemovalType.RightSuccessor:
                    var removed_color = succ.Color;
                    succ.Color = item.Color;
                    if (removed_color == RbColor.Black)
                        RemoveFixup(selfNextChild, selfNextParent);
                    break;
                default:
                    throw new NotImplementedException("Unknown Removal type");
            }
            return true;
        }

        private bool IsBlack(TreeNode<T> node)
            => node == null || node.Color == RbColor.Black;

        private TreeNode<T> Sibling(TreeNode<T> child, TreeNode<T> parent)
        {
            if (parent == null) return null;
            if (parent.Left == child) return parent.Right;
            else if (parent.Right == child) return parent.Left;
            else throw new ArgumentException();
        }

        // node is double black..
        private void RemoveFixup(TreeNode<T> extrablack, TreeNode<T> parent)
        {
            while (IsBlack(extrablack) && parent != null)
            {
                var sibling = Sibling(extrablack, parent);

                if (IsBlack(sibling.Left) && IsBlack(sibling.Right)) // case 1 and 2
                {
                    if (!IsBlack(sibling)) // case 1
                    {
                        if (parent.Left == extrablack) parent.ShiftLeft();
                        else parent.ShiftRight();
                        parent.Color = RbColor.Red;
                        sibling.Color = RbColor.Black;
                        if (Root == parent) Root = sibling;
                        continue;
                    }
                    else // case 2
                    {
                        sibling.Color = RbColor.Red;
                        extrablack = parent;
                        parent = extrablack.Parent;
                        continue;
                    }
                }
                //case 3
                else if (extrablack == parent.Left && IsBlack(sibling.Right)) // and left = Red
                {
                    sibling.ShiftRight();
                    sibling.Color = RbColor.Red;
                    sibling.Parent.Color = RbColor.Black;
                    continue;
                }
                else if (extrablack == parent.Right && IsBlack(sibling.Left)) // mirror of previous
                {
                    sibling.ShiftLeft();
                    sibling.Color = RbColor.Red;
                    sibling.Parent.Color = RbColor.Black;
                    continue;
                }
                // case 4
                else
                {
                    if (parent.Left == extrablack)
                    {
                        //parent.Right.Right is Red, so it is not null
                        parent.Right.Right.Color = RbColor.Black;
                        parent.ShiftLeft();
                    }
                    else
                    {
                        parent.Left.Left.Color = RbColor.Black;
                        parent.ShiftRight();
                    }
                    if (Root == parent)
                        Root = sibling;

                    parent.Parent.Color = parent.Color;
                    parent.Color = RbColor.Black;
                    Depth++;
                    extrablack = Root;
                    parent = null;
                }
            }
            if (extrablack == null) // should only reacheable when no element left
            {
                if (parent != null) throw new InvalidOperationException("Tree has invalid state");
                Depth = 0;
                Root = null;
                return;
            }
            if (extrablack.Color == RbColor.Red)
                extrablack.Color = RbColor.Black;
            else
                Depth--; // node is already root

        }

    }
}
