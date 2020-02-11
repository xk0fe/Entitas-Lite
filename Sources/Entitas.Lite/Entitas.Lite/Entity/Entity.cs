using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
//using Entitas.Utils;

namespace Entitas
{

	/// Use context.CreateEntity() to create a new entity and
	/// entity.Destroy() to destroy it.
	/// You can add, replace and remove IComponent to an entity.
	public partial class Entity
	{
        /// Occurs when a component gets added.
        /// All event handlers will be removed when
        /// the entity gets destroyed by the context.
        //public event EntityComponentChanged OnComponentAdded;

        /// Occurs when a component gets removed.
        /// All event handlers will be removed when
        /// the entity gets destroyed by the context.
        //public event EntityComponentChanged OnComponentRemoved;

		/*
        /// Occurs when a component gets replaced.
        /// All event handlers will be removed when
        /// the entity gets destroyed by the context.
        public event EntityComponentReplaced OnComponentReplaced;

        /// Occurs when an entity gets released and is not retained anymore.
        /// All event handlers will be removed when
        /// the entity gets destroyed by the context.
        public event EntityEvent OnEntityReleased;
		*/

        /// Occurs when calling entity.Destroy().
        /// All event handlers will be removed when
        /// the entity gets destroyed by the context.
        //public event EntityEvent OnDestroyEntity;

        /// The total amount of components an entity can possibly have.
        public int totalComponents => _totalComponents;

		/// The contextInfo is set by the context which created the entity and
		/// contains information about the context.
		/// It's used to provide better error messages.
		public ContextInfo contextInfo => _contextInfo;

		/// Each entity has its own unique id which will be set by the context when you create the entity.
		public int id => _id;

		/// The context manages the state of an entity. Active entities are enabled, destroyed entities are not.
		public bool enabled => _enabled;

		/// Optional name of Entity
		public string name { get => _name; set { _name = value; _toStringCache = null; } }

		private int _id;
		private bool _enabled;
		private string _name;

		private int _totalComponents;
		private IComponent[] _components;
		private BitArray _componentsMask;

		private IComponentPool[] _componentPools;


		private ContextInfo _contextInfo;

		//IAERC _aerc;

		//IComponent[] _componentsCache;
		//int[] _componentIndicesCache;

		//StringBuilder _toStringBuilder;

		private string _toStringCache;

		/*
		public void Initialize(int creationIndex, int totalComponents, Stack<IComponent>[] componentPools, ContextInfo contextInfo = null, IAERC aerc = null)
		{
			Reactivate(creationIndex);

			_totalComponents = totalComponents;
			_components = new IComponent[totalComponents];
			_componentPools = componentPools;

			_contextInfo = contextInfo ?? createDefaultContextInfo();
			_aerc = aerc ?? new SafeAERC(this);
		}
		*/

		/*
		ContextInfo createDefaultContextInfo()
		{
			var componentNames = new string[totalComponents];
			for (int i = 0; i < componentNames.Length; i++)
			{
				componentNames[i] = i.ToString();
			}

			return new ContextInfo("No Context", componentNames, null);
		}
		*/

		public void Reactivate(int creationIndex)
		{
			_id = creationIndex;
			_enabled = true;
		}

		/// Adds a component at the specified index.
		/// If already exists, return the old component.
		public IComponent AddComponent(int index)
		{
			if (!_enabled)
				throw new EntityIsNotEnabledException($"Cannot add component {_contextInfo.componentNames[index]} to {this} !");

			// If exists, return the old component
			var component = _components[index];
			if (component != null)
				return component;

			// Create from pool
			component = _componentPools[index].Get(); 

			// Add to component list
			_components[index] = component;
			_componentsMask[index] = true;

			//OnComponentAdded?.Invoke(this, index, component);

			return component;
		}

		/// Removes a component at the specified index if exists.
		public void RemoveComponent(int index)
		{
			if (!_enabled)
				throw new EntityIsNotEnabledException($"Cannot remove component {_contextInfo.componentNames[index]} from {this} !");

			RemoveComponentImpl(index);
		}

		/// Removes all components.
		public void RemoveAllComponents()
		{
			if (!_enabled)
				throw new EntityIsNotEnabledException($"Cannot remove all components from {this} !");

			for (int i = 0; i < _components.Length; i++)
				RemoveComponentImpl(i);
		}

		private void RemoveComponentImpl(int index)
		{
			var component = _components[index];
			if (component == null)
				return;

			// Remove from component list
			_components[index] = null;
			_componentsMask[index] = false;

			//OnComponentRemoved?.Invoke(this, index, component);

			// Return to pool
			_componentPools[index].Return(component);
		}

		/*
		/// Replaces an existing component at the specified index
		/// or adds it if it doesn't exist yet.
		/// The prefered way is to use the
		/// generated methods from the code generator.
		public void ReplaceComponent(int index, IComponent component)
		{
			if (!_enabled)
			{
				throw new EntityIsNotEnabledException(
					"Cannot replace component '" +
					_contextInfo.componentNames[index] + "' on " + this + "!"
				);
			}

			if (HasComponent(index))
			{
				replaceComponent(index, component);
			}
			else if (component != null)
			{
				AddComponent(index, component);
			}
		}

		void replaceComponent(int index, IComponent replacement)
		{
			//_toStringCache = null;
			var previousComponent = _components[index];
			if (replacement != previousComponent)
			{
				_components[index] = replacement;
				_componentsCache = null;
				if (replacement != null)
				{
					if (OnComponentReplaced != null)
					{
						OnComponentReplaced(this, index, previousComponent, replacement);
					}
				}
				else
				{
					_componentIndicesCache = null;
					if (OnComponentRemoved != null)
					{
						OnComponentRemoved(this, index, previousComponent);
					}
				}

				// Reset before return to pool
				var resetablePrevComponent = previousComponent as IResetable;
				if (resetablePrevComponent != null)
				{
					resetablePrevComponent.Reset();
				}

				GetComponentPool(index).Push(previousComponent);

			}
			else
			{
				if (OnComponentReplaced != null)
				{
					OnComponentReplaced(this, index, previousComponent, replacement);
				}
			}
		}
		*/

		/// Returns a component at the specified index.
		/// You can only get a component at an index if it exists.
		public IComponent GetComponent(int index)
		{
			return _components[index];
		}

		/*
        /// Returns all added components.
        public IComponent[] GetComponents() {
            if (_componentsCache == null) {
                var components = EntitasCache.GetIComponentList();

                    for (int i = 0; i < _components.Length; i++) {
                        var component = _components[i];
                        if (component != null) {
                            components.Add(component);
                        }
                    }

                    _componentsCache = components.ToArray();

                EntitasCache.PushIComponentList(components);
            }

            return _componentsCache;
        }

        /// Returns all indices of added components.
        public int[] GetComponentIndices() {
            if (_componentIndicesCache == null) {
                var indices = EntitasCache.GetIntList();

                    for (int i = 0; i < _components.Length; i++) {
                        if (_components[i] != null) {
                            indices.Add(i);
                        }
                    }

                    _componentIndicesCache = indices.ToArray();

                EntitasCache.PushIntList(indices);
            }

            return _componentIndicesCache;
        }
		*/

		/// Determines whether this entity has a component at the specified index.
		public bool HasComponent(int index)
		{
			return GetComponent(index) != null;
		}

		/*
		/// Determines whether this entity has components at all the specified indices.
		public bool HasComponents(int[] indices)
		{
			for (int i = 0; i < indices.Length; i++)
			{
				if (GetComponent(indices[i]) == null)
					return false;
			}

			return true;
		}

		/// Determines whether this entity has a component at any of the specified indices.
		public bool HasAnyComponent(int[] indices)
		{
			for (int i = 0; i < indices.Length; i++)
			{
				if (GetComponent(indices[i]) != null)
					return true;
			}

			return false;
		}
		*/


		/*
        /// Returns the componentPool for the specified component index.
        /// componentPools is set by the context which created the entity and
        /// is used to reuse removed components.
        /// Removed components will be pushed to the componentPool.
        /// Use entity.CreateComponent(index, type) to get a new or
        /// reusable component from the componentPool.
        public Stack<IComponent> GetComponentPool(int index) {
            var componentPool = _componentPools[index];
            if (componentPool == null) {
                componentPool = new Stack<IComponent>();
                _componentPools[index] = componentPool;
            }

            return componentPool;
        }

        /// Returns a new or reusable component from the componentPool
        /// for the specified component index.
        public IComponent CreateComponent(int index, Type type) {
            var componentPool = GetComponentPool(index);
            return componentPool.Count > 0
                        ? componentPool.Pop()
                        : (IComponent)Activator.CreateInstance(type);
        }

        /// Returns a new or reusable component from the componentPool
        /// for the specified component index.
        public T CreateComponent<T>(int index) where T : new() {
            var componentPool = GetComponentPool(index);
            return componentPool.Count > 0 ? (T)componentPool.Pop() : new T();
        }

        /// Returns the number of objects that retain this entity.
        public int retainCount { get { return _aerc.retainCount; } }

        /// Retains the entity. An owner can only retain the same entity once.
        /// Retain/Release is part of AERC (Automatic Entity Reference Counting)
        /// and is used internally to prevent pooling retained entities.
        /// If you use retain manually you also have to
        /// release it manually at some point.
        public void Retain(object owner) {
            _aerc.Retain(owner);
            //_toStringCache = null;
        }

        /// Releases the entity. An owner can only release an entity
        /// if it retains it.
        /// Retain/Release is part of AERC (Automatic Entity Reference Counting)
        /// and is used internally to prevent pooling retained entities.
        /// If you use retain manually you also have to
        /// release it manually at some point.
        public void Release(object owner) {
            _aerc.Release(owner);
            //_toStringCache = null;

            if (_aerc.retainCount == 0) {
                if (OnEntityReleased != null) {
                    OnEntityReleased(this);
                }
            }
        }
		*/

		// Dispatches OnDestroyEntity which will start the destroy process.
		public void Destroy()
		{
			if (!_enabled)
				throw new EntityIsNotEnabledException($"Cannot destroy {this} !");

			//OnDestroyEntity?.Invoke(this);
		}

		/*
		// This method is used internally. Don't call it yourself.
		// Use entity.Destroy();
		public void InternalDestroy()
		{
			_enabled = false;
			_name = null;
			_toStringCache = null;

			RemoveAllComponents();

			OnComponentAdded = null;
			OnComponentReplaced = null;
			OnComponentRemoved = null;
			OnDestroyEntity = null;
		}

		// Do not call this method manually. This method is called by the context.
		public void RemoveAllOnEntityReleasedHandlers()
		{
			OnEntityReleased = null;
		}
		*/

		/// Returns a cached string to describe the entity
		public override string ToString()
		{
			if (_toStringCache == null)
				_toStringCache = $"Entity({_id}) {_name}";

			return _toStringCache;
		}
	}
}