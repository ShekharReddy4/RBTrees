using System;
using System.Collections.Generic;
using System.Text;

namespace RedBlackTrees
{
    public class TreeNode<T>
    {
        public RbColor Color; // for RbTree B = 1, R = 0.
        public T Item;
        public TreeNode<T> Left;
        public TreeNode<T> Right;
        public TreeNode<T> Prev;
        public TreeNode<T> Next;
        public TreeNode<T> Parent;

        public TreeNode()
        {
            this.Item = default(T);
        }

        public TreeNode(T item)
        {
            this.Item = item;
        }

        public void AddChildLeft(TreeNode<T> item)
            => AddChild(item, this, NodeDirection.Left);
        public void AddChildRight(TreeNode<T> item)
            => AddChild(item, this, NodeDirection.Right);
        public static void AddChild(TreeNode<T> item, TreeNode<T> parent, NodeDirection direction)
        {
            if (direction == NodeDirection.Left)
            {
                var pp = parent.Prev;
                if (pp != null) pp.Next = item;
                item.Next = parent;
                parent.Prev = item;
                item.Prev = pp;
                item.Parent = parent;
                parent.Left = item;
            }
            else if (direction == NodeDirection.Right)
            {
                var pn = parent.Next;
                if (pn != null) pn.Prev = item;
                item.Prev = parent;
                parent.Next = item;
                item.Next = pn;
                item.Parent = parent;
                parent.Right = item;
            }
            else
                throw new ArgumentException("Unknown Direction");
        }

        public TreeNode<T> RemoveSelf() => Remove(this, out _, out _, out _);
        public TreeNode<T> RemoveSelf(out TreeRemovalType type,
            out TreeNode<T> selfNextParent, out TreeNode<T> selfNextChild)
            => Remove(this, out type, out selfNextParent, out selfNextChild);
        
        
        
        /// <summary>
        /// Remove node 'self' and returns its successor (upgrade right-based)
        /// </summary>
        /// <param name="self">Node to remove</param>
        /// <param name="type">Type of Removal. Case of LeftNull, RightNull, or Successor</param>
        /// <param name="selfNextParent">
        /// The parent of replaced node (Sucessor case), and null otherwise
        /// </param>
        /// <param name="selfNextChild">
        /// The (right) child of replaced node (Sucessor case), and null otherwise
        /// </param>
        /// <returns>Node at the original self position after deletion</returns>
        public static TreeNode<T> Remove(TreeNode<T> self, out TreeRemovalType type,
            out TreeNode<T> selfNextParent, out TreeNode<T> selfNextChild)
        {
            if (self == null)
            {
                throw new ArgumentNullException();
            }

            TreeNode<T> succ;
            if (self.Left == null)
            {
                succ = self.Right;
                selfNextParent = null;
                selfNextChild = null;
                LinkParent(self.Parent, self.Right, self);
                type = TreeRemovalType.LeftNull;
            }
            else if (self.Right == null)
            {
                succ = self.Left;
                selfNextParent = null;
                selfNextChild = null;
                LinkParent(self.Parent, self.Left, self);
                type = TreeRemovalType.RightNull;
            }
            else
            {
                succ = self.Next; // must be nonnull
                type = TreeRemovalType.RightSuccessor;

                if (succ == null || succ.Left != null)
                    throw new ArgumentException("Invalid State: Tree has both child " +
                                                "but successor is null or successor has left child");
                selfNextChild = succ.Right;
                selfNextParent = succ.Parent;

                LinkParent(selfNextParent, selfNextChild, succ);
                LinkParent(succ, self.Left, NodeDirection.Left);
                LinkParent(succ, self.Right, NodeDirection.Right);
                LinkParent(self.Parent, succ, self);

                if (selfNextParent == self)
                    selfNextParent = succ;
            }
            RemoveLink(self); // now collapse linked list
            return succ;
        }

        public void RemoveLink() => RemoveLink(this);
        public static void RemoveLink(TreeNode<T> self)
        {
            var ps = self.Prev;
            var ns = self.Next;
            if (ps != null) ps.Next = ns;
            if (ns != null) ns.Prev = ps;
        }

        public TreeNode<T> ShiftRight() => ShiftRight(this);
        // Shift LR node to RL. If Left is null, throw InvalidOperation Exception.
        // note LL or R can be null
        public static TreeNode<T> ShiftRight(TreeNode<T> parent)
        {
            TreeNode<T> left = parent.Left;
            if (left == null) throw new InvalidOperationException("Left Node cannot be null.");
            LinkParent(parent.Parent, left, parent);
            LinkParent(parent, left.Right, NodeDirection.Left);
            LinkParent(left, parent, NodeDirection.Right);

            return left;
        }

        public TreeNode<T> ShiftLeft() => ShiftLeft(this);
        public static TreeNode<T> ShiftLeft(TreeNode<T> parent)
        {
            TreeNode<T> right = parent.Right;
            if (right == null) throw new InvalidOperationException("Right Node cannot be null.");
            LinkParent(parent.Parent, right, parent);
            LinkParent(parent, right.Left, NodeDirection.Right);
            LinkParent(right, parent, NodeDirection.Left);

            return right;
        }

        private static void LinkParent(TreeNode<T> parent, TreeNode<T> child, NodeDirection direction)
        {
            if (child != null) child.Parent = parent;
            if (direction == NodeDirection.Left)
                parent.Left = child;
            else if (direction == NodeDirection.Right)
                parent.Right = child;
            else { if (parent != null && child != null) throw new ArgumentException("Invalid Direction"); }
        }

        private static void LinkParent(TreeNode<T> parent, TreeNode<T> newChild, TreeNode<T> prevChild)
        {
            if (newChild != null)
                newChild.Parent = parent;
            if (parent == null) return;

            if (parent.Left == prevChild)
                parent.Left = newChild;
            else if (parent.Right == prevChild)
                parent.Right = newChild;
            else
                throw new ArgumentException("Invalid Parameters: prevChild is not child");
            prevChild.Parent = null;
        }


    }
}
