﻿namespace SampleLibrary.Proxy
{
    public class DerivedClassWithVirtualByRefMethod : ClassWithVirtualByRefMethod
    {
        public override void ByRefMethod(ref int a)
        {
            a = 54321;
        }
    }
}