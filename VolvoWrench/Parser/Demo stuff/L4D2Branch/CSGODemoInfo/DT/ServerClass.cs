using System;
using System.Collections.Generic;
using VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DP;

namespace VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DT
{
    internal class ServerClass : IDisposable
    {
        public List<ServerClass> BaseClasses = new List<ServerClass>();
        public int ClassID;
        public int DataTableID;
        public string DTName;
        public List<FlattenedPropEntry> FlattenedProps = new List<FlattenedPropEntry>();
        public string Name;

        public void Dispose()
        {
            OnNewEntity = null;
        }

        public event EventHandler<EntityCreatedEventArgs> OnNewEntity;

        internal void AnnounceNewEntity(Entity e)
        {
            if (OnNewEntity != null) OnNewEntity(this, new EntityCreatedEventArgs(this, e));
        }

        public override string ToString()
        {
            return Name + " | " + DTName;
        }
    }

    internal class FlattenedPropEntry
    {
        public FlattenedPropEntry(string propertyName, SendTableProperty prop, SendTableProperty arrayElementProp)
        {
            Prop = prop;
            ArrayElementProp = arrayElementProp;
            PropertyName = propertyName;
        }

        public SendTableProperty Prop { get; }
        public SendTableProperty ArrayElementProp { get; }
        public string PropertyName { get; }

        public override string ToString()
        {
            return string.Format("[FlattenedPropEntry: PropertyName={2}, Prop={0}, ArrayElementProp={1}]", Prop,
                ArrayElementProp, PropertyName);
        }
    }

    internal class ExcludeEntry
    {
        public ExcludeEntry(string varName, string dtName, string excludingDT)
        {
            VarName = varName;
            DTName = dtName;
            ExcludingDT = excludingDT;
        }

        public string VarName { get; }
        public string DTName { get; }
        public string ExcludingDT { get; }
    }


    internal class EntityCreatedEventArgs : EventArgs
    {
        public EntityCreatedEventArgs(ServerClass c, Entity e)
        {
            Class = c;
            Entity = e;
        }

        public ServerClass Class { get; }
        public Entity Entity { get; }
    }
}