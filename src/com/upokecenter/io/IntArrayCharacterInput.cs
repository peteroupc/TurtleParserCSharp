/*
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
*/
namespace com.upokecenter.io {
using System;
using System.IO;


public sealed class IntArrayCharacterInput : ICharacterInput {

	int pos;
	int[] ilist;

	public IntArrayCharacterInput(int[] ilist){
		this.ilist=ilist;
	}

	public int read()  {
		int[] arr=this.ilist;
		if(pos<this.ilist.Length)
			return arr[pos++];
		return -1;
	}

	public int read(int[] buf, int offset, int unitCount)  {
		if(offset<0 || unitCount<0 || offset+unitCount>buf.Length)
			throw new ArgumentOutOfRangeException();
		if(unitCount==0)return 0;
		int[] arr=this.ilist;
		int size=this.ilist.Length;
		int count=0;
		while(pos<size && unitCount>0){
			buf[offset]=arr[pos];
			offset++;
			count++;
			unitCount--;
			pos++;
		}
		return count==0 ? -1 : count;
	}

}

}
