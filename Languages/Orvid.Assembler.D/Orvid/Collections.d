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
	
	//public alias Add add;
	public void Add(T val) @safe pure nothrow
	{
		if (this.nextIdx >= this.innerArray.length)
		{
			this.innerArray.length = this.innerArray.length << 1;
		}
		this.innerArray[nextIdx++] = val;
	}
	
	public T opIndex(int idx) @safe pure nothrow
	in
	{
		assert(idx < nextIdx);
		assert(idx >= 0);
	}
	body
	{
		return innerArray[idx];
	}
	
	public void opIndexAssign(T val, int idx) @safe pure nothrow
	in
	{
		assert(idx < nextIdx);
		assert(idx >= 0);
	}
	body
	{
		this.innerArray[idx] = val;
	}
	
	public List!T opSlice(int x, int y) @safe pure nothrow
	in
	{
		assert(x < nextIdx);
		assert(y < nextIdx);
		assert(x >= 0);
		assert(y >= 0);
	}
	body
	{
		if (y > x)
		{
			int b = x;
			x = y;
			y = b;
		}
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