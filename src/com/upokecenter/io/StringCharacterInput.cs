/*
Written in 2013 by Peter Occil.  Released to the public domain.
Public domain dedication: http://creativecommons.org/publicdomain/zero/1.0/
 */
namespace com.upokecenter.io {
using System;
using System.Text;
using System.IO;



public sealed class StringCharacterInput : ICharacterInput {

	string str=null;
	int pos=0;
	bool strict=false;
	public StringCharacterInput(string str) : this(str,false) {
	}
	public StringCharacterInput(string str, bool strict){
		if(str==null)
			throw new ArgumentException();
		this.str=str;
		this.strict=strict;
	}

	public int read()  {
		if(pos<str.Length){
			int c=str[pos];
			if(c>=0xD800 && c<=0xDBFF && pos+1<str.Length &&
					str[pos+1]>=0xDC00 && str[pos+1]<=0xDFFF){
				// Get the Unicode code point for the surrogate pair
				c=0x10000+(c-0xD800)*0x400+(str[pos+1]-0xDC00);
				pos++;
			} else if(strict && c>=0xD800 && c<=0xDFFF)
				throw new IOException("",new DecoderFallbackException());
			pos++;
			return c;
		}
		return -1;
	}

	public int read(int[] buf, int offset, int unitCount)  {
		if(offset<0 || unitCount<0 || offset+unitCount>buf.Length)
			throw new ArgumentOutOfRangeException();
		if(unitCount==0)return 0;
		int count=0;
		while(pos<str.Length && unitCount>0){
			int c=str[pos];
			if(c>=0xD800 && c<=0xDBFF && pos+1<str.Length &&
					str[pos+1]>=0xDC00 && str[pos+1]<=0xDFFF){
				// Get the Unicode code point for the surrogate pair
				c=0x10000+(c-0xD800)*0x400+(str[pos+1]-0xDC00);
				pos++;
			} else if(strict && c>=0xD800 && c<=0xDFFF)
				throw new IOException("",new DecoderFallbackException());
			buf[offset]=c;
			offset++;
			unitCount--;
			count++;
			pos++;
		}
		return count==0 ? -1 : count;
	}

}

}
