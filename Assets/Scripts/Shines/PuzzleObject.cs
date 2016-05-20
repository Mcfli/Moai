using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PuzzleObject : MonoBehaviour, IEquatable<PuzzleObject>
{
    public string ID;
    public Material image;
    public Material imageGlowing;

    public bool Equals(PuzzleObject other)
    {
        if(ID == "universal" || other.ID == "universal") return true;
        return ID.Equals(other.ID);
    }

    public override bool Equals(object other){
        PuzzleObject po = other as PuzzleObject;
        if (po != null)
            return Equals(po);
        return false;
    }

    public override int GetHashCode(){
        return ID.GetHashCode();
    }

}
