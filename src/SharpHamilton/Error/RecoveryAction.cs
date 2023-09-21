using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huarui.STARLine
{
    public enum RecoveryAction
    {
        [ReocveryTitle(0)]
        None=0,
        [ReocveryTitle(436)]
        Abort =1,
        [ReocveryTitle(370)]
        Cancel =2,
        [ReocveryTitle(418)]
        Initialize =3,
        [ReocveryTitle(420)]
        Repeat =4,
        [ReocveryTitle(416)]
        Exclude =5,
        [ReocveryTitle(426)]
        Waste =6,
        [ReocveryTitle(434)]
        Air =7,
        [ReocveryTitle(432)]
        Bottom =8,
        [ReocveryTitle(428)]
        Continue =9,
        [ReocveryTitle(430)]
        Barcode =10,
        [ReocveryTitle(424)]
        Next =11,
        [ReocveryTitle(818)]
        Available =12,
        [ReocveryTitle(1482)]
        Refill =13
    }
    /// <summary>
    /// Recovery Title Attribute to find the recovery action
    /// </summary>
    class ReocveryTitleAttribute : Attribute
    {
        public int Title { get; private set; }
        public int Description { get; private set; }
        public ReocveryTitleAttribute(int title)
        {
            Title = title;
        }
        public ReocveryTitleAttribute(int title, int description)
        {
            Title = title;
            Description = description;
        }
    }
}
