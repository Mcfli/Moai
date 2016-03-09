using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PuzzleObject : MonoBehaviour, IEquatable<PuzzleObject>
{
    public string ID;
    public Texture2D image;
    public bool Equals(PuzzleObject other)
    {
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
