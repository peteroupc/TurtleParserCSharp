/*
Written in 2013 by Peter Occil.  Released to the public domain.
Public domain dedication: http://creativecommons.org/publicdomain/zero/1.0/
 */
namespace com.upokecenter.io {
using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;


/**
 * 
 * A character input stream where additional inputs can be stacked on
 * top of it.  It supports advanced marking capabilities.
 * 
 * @author Peter
 *
 */
public sealed class StackableCharacterInput : IMarkableCharacterInput {


	private class InputAndBuffer : ICharacterInput {

		int[] buffer;
		ICharacterInput charInput;
		int pos=0;

		public InputAndBuffer(ICharacterInput charInput, int[] buffer, int offset, int length){
			this.charInput=charInput;
			if(length>0){
				this.buffer=new int[length];
				Array.Copy(buffer,offset,this.buffer,0,length);
			} else {
				this.buffer=null;
			}
		}

		public int read()  {
			if(charInput!=null){
				int c=charInput.read();
				if(c>=0)return c;
				charInput=null;
			}
			if(buffer!=null){
				if(pos<buffer.Length)
					return buffer[pos++];
				buffer=null;
			}
			return -1;
		}

		public int read(int[] buf, int offset, int unitCount)
				 {
			if((buf)==null)throw new ArgumentNullException("buf");
			if((offset)<0)throw new ArgumentOutOfRangeException("offset not greater or equal to 0 ("+Convert.ToString(offset,CultureInfo.InvariantCulture)+")");
			if((unitCount)<0)throw new ArgumentOutOfRangeException("unitCount not greater or equal to 0 ("+Convert.ToString(unitCount,CultureInfo.InvariantCulture)+")");
			if((offset+unitCount)>buf.Length)throw new ArgumentOutOfRangeException("offset+unitCount not less or equal to "+Convert.ToString(buf.Length,CultureInfo.InvariantCulture)+" ("+Convert.ToString(offset+unitCount,CultureInfo.InvariantCulture)+")");
			if(unitCount==0)return 0;
			int count=0;
			if(charInput!=null){
				int c=charInput.read(buf,offset,unitCount);
				if(c<=0){
					charInput=null;
				} else {
					offset+=c;
					unitCount-=c;
					count+=c;
				}
			}
			if(buffer!=null){
				int c=Math.Min(unitCount,this.buffer.Length-pos);
				if(c>0){
					Array.Copy(this.buffer,pos,buf,offset,c);
				}
				pos+=c;
				count+=c;
				if(c==0){
					buffer=null;
				}
			}
			return (count==0) ? -1 : count;
		}

	}

	int pos=0;
	int endpos=0;
	bool haveMark=false;
	int[] buffer=null;
	IList<ICharacterInput> stack=new List<ICharacterInput>();

	public StackableCharacterInput(ICharacterInput source) {
		this.stack.Add(source);
	}

	public int getMarkPosition(){
		return pos;
	}

	public void moveBack(int count)  {
		if((count)<0)throw new ArgumentOutOfRangeException("count not greater or equal to 0 ("+Convert.ToString(count,CultureInfo.InvariantCulture)+")");
		if(haveMark && pos>=count){
			pos-=count;
			return;
		}
		throw new IOException();
	}

	public void pushInput(ICharacterInput input){
		if((input)==null)throw new ArgumentNullException("input");
		// Move unread characters in buffer, since this new
		// input sits on top of the existing input
		stack.Add(new InputAndBuffer(input,buffer,pos,endpos-pos));
		endpos=pos;
	}

	public int read() {
		if(haveMark){
			// Read from buffer
			if(pos<endpos)
				return buffer[pos++];
			//Console.WriteLine(this);
			// End pos is smaller than buffer size, fill
			// entire buffer if possible
			if(endpos<buffer.Length){
				int count=readInternal(buffer,endpos,buffer.Length-endpos);
				if(count>0) {
					endpos+=count;
				}
			}
			// Try reading from buffer again
			if(pos<endpos)
				return buffer[pos++];
			//Console.WriteLine(this);
			// No room, read next character and put it in buffer
			int c=readInternal();
			if(c<0)return c;
			if(pos>=buffer.Length){
				int[] newBuffer=new int[buffer.Length*2];
				Array.Copy(buffer,0,newBuffer,0,buffer.Length);
				buffer=newBuffer;
			}
			//Console.WriteLine(this);
			buffer[pos++]=(byte)(c&0xFF);
			endpos++;
			return c;
		} else
			return readInternal();
	}

	public int read(int[] buf, int offset, int unitCount)  {
		if(haveMark){
			if((buf)==null)throw new ArgumentNullException("buf");
			if((offset)<0)throw new ArgumentOutOfRangeException("offset not greater or equal to 0 ("+Convert.ToString(offset,CultureInfo.InvariantCulture)+")");
			if((unitCount)<0)throw new ArgumentOutOfRangeException("unitCount not greater or equal to 0 ("+Convert.ToString(unitCount,CultureInfo.InvariantCulture)+")");
			if((offset+unitCount)>buf.Length)throw new ArgumentOutOfRangeException("offset+unitCount not less or equal to "+Convert.ToString(buf.Length,CultureInfo.InvariantCulture)+" ("+Convert.ToString(offset+unitCount,CultureInfo.InvariantCulture)+")");
			if(unitCount==0)return 0;
			// Read from buffer
			if(pos+unitCount<=endpos){
				Array.Copy(buffer,pos,buf,offset,unitCount);
				pos+=unitCount;
				return unitCount;
			}
			// End pos is smaller than buffer size, fill
			// entire buffer if possible
			int count=0;
			if(endpos<buffer.Length){
				count=readInternal(buffer,endpos,buffer.Length-endpos);
				//Console.WriteLine("%s",this);
				if(count>0) {
					endpos+=count;
				}
			}
			int total=0;
			// Try reading from buffer again
			if(pos+unitCount<=endpos){
				Array.Copy(buffer,pos,buf,offset,unitCount);
				pos+=unitCount;
				return unitCount;
			}
			// expand the buffer
			if(pos+unitCount>buffer.Length){
				int[] newBuffer=new int[(buffer.Length*2)+unitCount];
				Array.Copy(buffer,0,newBuffer,0,buffer.Length);
				buffer=newBuffer;
			}
			count=readInternal(buffer, endpos, Math.Min(unitCount,buffer.Length-endpos));
			if(count>0) {
				endpos+=count;
			}
			// Try reading from buffer a third time
			if(pos+unitCount<=endpos){
				Array.Copy(buffer,pos,buf,offset,unitCount);
				pos+=unitCount;
				total+=unitCount;
			} else if(endpos>pos){
				Array.Copy(buffer,pos,buf,offset,endpos-pos);
				total+=(endpos-pos);
				pos=endpos;
			}
			return (total==0) ? -1 : total;
		} else
			return readInternal(buf, offset, unitCount);
	}

	private int readInternal()  {
		if(this.stack.Count==0)return -1;
		while(this.stack.Count>0){
			int index=this.stack.Count-1;
			int c=this.stack[index].read();
			if(c==-1){
				this.stack.RemoveAt(index);
				continue;
			}
			return c;
		}
		return -1;
	}

	private int readInternal(int[] buf, int offset, int unitCount)  {
		if(this.stack.Count==0)return -1;
		#if DEBUG
if(!(((buf)!=null) ))throw new InvalidOperationException("buf");
#endif
		#if DEBUG
if(!(((offset)>=0) ))throw new InvalidOperationException(("offset not greater or equal to 0 ("+Convert.ToString(offset,CultureInfo.InvariantCulture)+")"));
#endif
		#if DEBUG
if(!(((unitCount)>=0) ))throw new InvalidOperationException(("unitCount not greater or equal to 0 ("+Convert.ToString(unitCount,CultureInfo.InvariantCulture)+")"));
#endif
		#if DEBUG
if(!(((offset+unitCount)<=buf.Length) ))throw new InvalidOperationException(("offset+unitCount not less or equal to "+Convert.ToString(buf.Length,CultureInfo.InvariantCulture)+" ("+Convert.ToString(offset+unitCount,CultureInfo.InvariantCulture)+")"));
#endif
		if(unitCount==0)return 0;
		int count=0;
		while(this.stack.Count>0 && unitCount>0){
			int index=this.stack.Count-1;
			int c=this.stack[index].read(buf,offset,unitCount);
			if(c<=0){
				this.stack.RemoveAt(index);
				continue;
			}
			count+=c;
			unitCount-=c;
			if(unitCount==0){
				break;
			}
			this.stack.RemoveAt(index);
		}
		return count;
	}

	public int setHardMark(){
		if(buffer==null){
			buffer=new int[16];
			pos=0;
			endpos=0;
			haveMark=true;
		} else if(haveMark){
			// Already have a mark; shift buffer to the new mark
			if(pos>0 && pos<endpos){
				Array.Copy(buffer,pos,buffer,0,endpos-pos);
			}
			endpos-=pos;
			pos=0;
		} else {
			pos=0;
			endpos=0;
			haveMark=true;
		}
		return 0;
	}

	public void setMarkPosition(int pos) {
		if(!haveMark || pos<0 || pos>endpos)
			throw new IOException();
		this.pos=pos;
	}

	public int setSoftMark(){
		if(!haveMark){
			setHardMark();
		}
		return getMarkPosition();
	}

}

}
