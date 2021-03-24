using System;
using Unity.Mathematics;
using UnityEngine;

public struct ID : IEquatable<ID>
{
    public int X { get; }
    public int Y { get; }

    public ID(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public static ID operator -(ID valueOne, ID valueTwo) => new ID(valueOne.X - valueTwo.X, valueOne.Y - valueTwo.Y);

    public static ID operator -(ID valueOne, int2 valueTwo) => new ID(valueOne.X - valueTwo.x, valueOne.Y - valueTwo.y);

    public static ID operator +(ID valueOne, ID valueTwo) => new ID(valueOne.X + valueTwo.X, valueOne.Y + valueTwo.Y);

    public static ID operator +(ID valueOne, Vector2Int valueTwo) => new ID(valueOne.X + valueTwo.x, valueOne.Y + valueTwo.y);

    public static ID operator +(ID valueOne, int2 valueTwo) => new ID(valueOne.X + valueTwo.x, valueOne.Y + valueTwo.y);

    public static ID operator +(int2 valueTwo, ID valueOne) => new ID(valueOne.X + valueTwo.x, valueOne.Y + valueTwo.y);

    public static bool operator ==(ID valueOne, ID valueTwo) => (valueOne.X == valueTwo.X) && (valueOne.Y == valueTwo.Y);

    public static bool operator ==(ID valueOne, int2 valueTwo) => (valueOne.X == valueTwo.x) && (valueOne.Y == valueTwo.y);

    public static bool operator !=(ID valueOne, ID valueTwo) => (valueOne.X != valueTwo.X) || (valueOne.Y != valueTwo.Y);

    public static bool operator !=(ID valueOne, int2 valueTwo) => (valueOne.X != valueTwo.x) || (valueOne.Y != valueTwo.y);

    public static implicit operator ID(int2 input) => new ID(input.x, input.y);

    public static implicit operator int2(ID input) => new int2(input.X, input.Y);

    public bool Equals(ID other) => this == other;

    public override bool Equals(object obj) => (obj is ID id) && Equals(id);

    public override int GetHashCode() =>  (int) math.hash(this);

    public override string ToString() => this.X + " " + this.Y;
}