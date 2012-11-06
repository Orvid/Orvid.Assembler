module Orvid.Collections;

public final @safe pure struct List(T)
{
	private static const int DefaultInitialCapacity = 4;
	
	private int nextIdx = 0;
	private T[] innerArray;
	
	public @property int Count() @safe pure nothrow
	{
		return nextIdx;
	}
	
	public this(int initCapacity = DefaultInitialCapacity) @safe pure nothrow
	{
		innerArray = new T[initCapacity];
	}
	
	public alias Add add; // D Naming
	public void Add(T val) @safe pure nothrow
	{
		if (this.nextIdx >= this.innerArray.length)
		{
			this.innerArray.length = this.innerArray.length << 1;
		}
		this.innerArray[nextIdx++] = val;
	}
	
	public T opIndex(int idx) @safe pure
	{
		if (idx >= nextIdx || idx < 0)
			throw new Exception("Index out of range!");
		return innerArray[idx];
	}
	
	public void opIndexAssign(T val, int idx) @safe pure
	{
		if (idx >= nextIdx || idx < 0)
			throw new Exception("Index out of range!");
		this.innerArray[idx] = val;
	}
	
	public List!T opSlice(int x, int y)
	{
		if (y > x)
		{
			int b = x;
			x = y;
			y = b;
		}
		if (x >= nextIdx || y >= nextIdx || x < 0 || y < 0)
			throw new Exception("Index out of range!");
		List!T lst;
		lst.innerArray = this.innerArray[x..y];
		lst.nextIdx = cast(int)(y - x);
		return lst;
	}
	
	public bool opEquals(ref const List!T lst) @safe pure nothrow
	{
		return this.innerArray == lst.innerArray;
	}
}