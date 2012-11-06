/******************************************************************\
 *      Class Name:     RedBlackEnumerator
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *
 *      Modifications:
 *      -----------------------------------------------------------
 *      Date            Author          Modification
 *
\******************************************************************/
///<summary>
/// The RedBlackEnumerator class returns the keys or data objects of the treap in
/// sorted order. 
///</summary>
using System;
using System.Collections.Generic;

namespace DataTemple.Codeland.SearchTree
{
    public class RedBlackEnumerator<KeyType, DataType> : IEnumerator<KeyValuePair<KeyType, DataType>>
        where KeyType : IComparable<KeyType>
    {
		// the treap uses the stack to order the nodes
		private Stack<RedBlackNode<KeyType, DataType>> stack;
		// return in ascending order (true) or descending (false)
		private bool ascending;
		
		// key
		private KeyType ordKey;
		// the data or value associated with the key
		private DataType objValue;
		
        private RedBlackNode<KeyType, DataType> root;

		///<summary>
		///Key
		///</summary>
		public KeyType Key
		{
			get
            {
				return ordKey;
			}
			
			set
			{
				ordKey = value;
			}
		}
		///<summary>
		///Data
		///</summary>
		public DataType Value
		{
			get
            {
				return objValue;
			}
			
			set
			{
				objValue = value;
			}
		}
		
		public RedBlackEnumerator() 
        {
		}
		///<summary>
		/// Determine order, walk the tree and push the nodes onto the stack
		///</summary>
		public RedBlackEnumerator(RedBlackNode<KeyType, DataType> tnode, bool ascending) 
        {
			root = tnode;
			stack           = new Stack<RedBlackNode<KeyType, DataType>>();
			this.ascending  = ascending;
            Reset();
		}

        public void Reset()
        {
            stack.Clear();
            RedBlackNode<KeyType, DataType> tnode = root;

            // use depth-first traversal to push nodes into stack
            // the lowest node will be at the top of the stack
            if (ascending)
            {   // find the lowest node
                while (tnode != null)
                {
                    stack.Push(tnode);
                    tnode = tnode.Left;
                }
            }
            else
            {
                // the highest node will be at top of stack
                while (tnode != null)
                {
                    stack.Push(tnode);
                    tnode = tnode.Right;
                }
            }
        }

		///<summary>
		/// HasMoreElements
		///</summary>
		public bool HasMoreElements()
		{
			return (stack.Count > 0);
		}
		///<summary>
		/// NextNode
		///</summary>
        public RedBlackNode<KeyType, DataType> NextElement()
		{
			if(stack.Count == 0)
				throw(new RedBlackException("Element not found"));
			
			// the top of stack will always have the next item
			// get top of stack but don't remove it as the next nodes in sequence
			// may be pushed onto the top
			// the stack will be popped after all the nodes have been returned
            RedBlackNode<KeyType, DataType> node = stack.Peek();	//next node in sequence
			
            if(ascending)
            {
                if (node.Right == null)
                {	
                    // yes, top node is lowest node in subtree - pop node off stack 
                    RedBlackNode<KeyType, DataType> tn = stack.Pop();
                    // peek at right node's parent 
                    // get rid of it if it has already been used
                    while(HasMoreElements()&& stack.Peek().Right == tn)
                        tn = stack.Pop();
                }
                else
                {
                    // find the next items in the sequence
                    // traverse to left; find lowest and push onto stack
                    RedBlackNode<KeyType, DataType> tn = node.Right;
                    while (tn != null)
                    {
                        stack.Push(tn);
                        tn = tn.Left;
                    }
                }
            }
            else            // descending, same comments as above apply
            {
                if (node.Left == null)
                {
                    // walk the tree
                    RedBlackNode<KeyType, DataType> tn = stack.Pop();
                    while(HasMoreElements() && stack.Peek().Left == tn)
                        tn = stack.Pop();
                }
                else
                {
                    // determine next node in sequence
                    // traverse to left subtree and find greatest node - push onto stack
                    RedBlackNode<KeyType, DataType> tn = node.Left;
                    while (tn != null)
                    {
                        stack.Push(tn);
                        tn = tn.Right;
                    }
                }
            }
			
			// the following is for .NET compatibility (see MoveNext())
			Key     = node.Key;
			Value   = node.Data;

            return node;
		}
		///<summary>
		/// MoveNext
		/// For .NET compatibility
		///</summary>
		public bool MoveNext()
		{
			if(HasMoreElements())
			{
				NextElement();
				return true;
			}
			return false;
		}

        public object Current
        {
            get {
                return new KeyValuePair<KeyType, DataType>(ordKey, objValue);
            }
        }

        KeyValuePair<KeyType, DataType> IEnumerator<KeyValuePair<KeyType, DataType>>.Current
        {
            get
            {
                return new KeyValuePair<KeyType, DataType>(ordKey, objValue);
            }
        }

        void IDisposable.Dispose()
        {
        }

        public class DataOnly : IEnumerator<DataType>
        {
            private RedBlackEnumerator<KeyType, DataType> enumerator;

            public DataOnly() {
            }

            public DataOnly(RedBlackNode<KeyType, DataType> tnode, bool ascending)
            {
                enumerator = new RedBlackEnumerator<KeyType, DataType>(tnode, ascending);
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            public object Current
            {
                get
                {
                    return ((KeyValuePair<KeyType, DataType>)enumerator.Current).Value;
                }
            }
            
            DataType IEnumerator<DataType>.Current
            {
                get
                {
                    return ((KeyValuePair<KeyType, DataType>)enumerator.Current).Value;
                }
            }

            void IDisposable.Dispose()
            {
            }
        }
    }
}
