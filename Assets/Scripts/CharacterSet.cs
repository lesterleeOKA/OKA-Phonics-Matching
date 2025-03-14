using System;
using UnityEngine;

[Serializable]
public class CharacterSet
{
    public string name;
    public int playerNumber;
    public Color playerColor;
    public Texture defaultIcon;
    public Texture idlingTexture;
    public Texture[] walkingAnimationTextures;
}
