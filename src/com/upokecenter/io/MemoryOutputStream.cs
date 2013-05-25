/*
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
*/
namespace com.upokecenter.io {
using System;
using System.Globalization;
using System.IO;




public sealed class MemoryOutputStream : PeterO.Support.OutputStream {

	byte[] buffer=new byte[16];
	int pos=0;

	public int this[int i] { get { return get(i); }}
public int get(int index){
		if(index>=pos)return -1;
		return buffer[index];
	}

	public int length(){
		return pos;
	}

	public void reset(){
		pos=0;
	}

	public byte[] toByteArray(){
		byte[] bytes=new byte[pos];
		Array.Copy(buffer,0,bytes,0,pos);
		return bytes;
	}

	public PeterO.Support.InputStream toInputStream(){
		return new PeterO.Support.ByteArrayInputStream(buffer,0,pos);
	}

	public override sealed void Write(byte[] buf, int off, int len){
		if((buf)==null)throw new ArgumentNullException("buf");
		if((off)<0)throw new ArgumentOutOfRangeException("off not greater or equal to 0 ("+Convert.ToString(off,CultureInfo.InvariantCulture)+")");
		if((len)<0)throw new ArgumentOutOfRangeException("len not greater or equal to 0 ("+Convert.ToString(len,CultureInfo.InvariantCulture)+")");
		if((off+len)>buf.Length)throw new ArgumentOutOfRangeException("off+len not less or equal to "+Convert.ToString(buf.Length,CultureInfo.InvariantCulture)+" ("+Convert.ToString(off+len,CultureInfo.InvariantCulture)+")");
		if(pos+len>buffer.Length){
			byte[] newbuffer=new byte[Math.Max(pos+len+1024, buffer.Length*2)];
			Array.Copy(buffer,0,newbuffer,0,buffer.Length);
			buffer=newbuffer;
		}
		Array.Copy(buf,off,buffer,pos,len);
	}

	public override sealed void WriteByte(byte b)  {
		if(pos>=buffer.Length){
			byte[] newbuffer=new byte[Math.Max(pos+10,buffer.Length*2)];
			Array.Copy(buffer,0,newbuffer,0,buffer.Length);
			buffer=newbuffer;
		}
		buffer[pos++]=(byte)(b&0xFF);
	}
}

}
