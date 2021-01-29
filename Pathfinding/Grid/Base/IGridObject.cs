using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFindingAsteria
{
    public interface IGridObject 
    {
        bool GridContact(GridManager grid);

        void RemoveGrid(GridManager grid);


    }
}
