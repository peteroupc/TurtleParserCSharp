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


/**
 * 
 * An input stream that stores the first bytes of the stream
 * in a buffer and supports rewinding to the beginning of the stream.
 * However, when the buffer is disabled, no further bytes
 * are put into the buffer, but any remaining bytes in the buffer
 * will still be used until it's exhausted.
 * 
 * @author Peter O.
 *
 */
public sealed class ConditionalBufferInputStream : PeterO.Support.InputStream {

  private byte[] buffer=null;
  private int pos=0;
  private int endpos=0;
  private bool disabled=false;
  private long markpos=-1;
  private int posAtMark=0;
  private long marklimit=0;
  private PeterO.Support.InputStream stream=null;


  public ConditionalBufferInputStream(PeterO.Support.InputStream input) {
    this.stream=input;
    this.buffer=new byte[1024];
  }

  public override sealed int available() {
    if(isDisabled())
      return stream.available();
    return 0;
  }

  public override sealed void Close() {
    disabled=true;
    pos=0;
    endpos=0;
    buffer=null;
    markpos=-1;
    stream.Close();
  }

  /**
   * Disables buffering of future bytes read from
   * the underlying stream.  However, any bytes already
   * buffered can still be read until the buffer is exhausted.
   * After the buffer is exhausted, this stream will fully
   * delegate to the underlying stream.
   * 
   */
  public void disableBuffer(){
    disabled=true;
    if(buffer!=null && isDisabled()){
      buffer=null;
    }
  }

  public int doRead(byte[] buffer, int offset, int byteCount) {
    if(markpos<0)
      return readInternal(buffer,offset,byteCount);
    else {
      if(isDisabled())
        return stream.Read(buffer,offset,byteCount);
      int c=readInternal(buffer,offset,byteCount);
      if(c>0 && markpos>=0){
        markpos+=c;
        if(markpos>marklimit){
          marklimit=0;
          markpos=-1;
          if(this.buffer!=null && isDisabled()){
            this.buffer=null;
          }
        }
      }
      return c;
    }
  }

  private bool isDisabled(){
    if(disabled){
      if(markpos>=0 && markpos<marklimit)
        return false;
      if(pos<endpos)
        return false;
      return true;
    }
    return false;
  }

  public override sealed void mark(int limit){
    //Console.WriteLine("mark %d: %s",limit,isDisabled());
    if(isDisabled()){
      stream.mark(limit);
      return;
    }
    if(limit<0)
      throw new ArgumentException();
    markpos=0;
    posAtMark=pos;
    marklimit=limit;
  }

  public override sealed bool markSupported(){
    return true;
  }

  public override sealed int ReadByte() {
    if(markpos<0)
      return readInternal();
    else {
      if(isDisabled())
        return stream.ReadByte();
      int c=readInternal();
      if(c>=0 && markpos>=0){
        markpos++;
        if(markpos>marklimit){
          marklimit=0;
          markpos=-1;
          if(buffer!=null && isDisabled()){
            buffer=null;
          }
        }
      }
      return c;
    }
  }

  public override sealed int Read(byte[] buffer, int offset, int byteCount) {
    return doRead(buffer,offset,byteCount);
  }

  private int readInternal()  {
    // Read from buffer
    if(pos<endpos)
      return (buffer[pos++]&0xFF);
    //if(buffer!=null)
    //Console.WriteLine("buffer %s end=%s len=%s",pos,endpos,buffer.Length);
    if(disabled)
      // Buffering disabled, so read directly from stream
      return stream.ReadByte();
    // End pos is smaller than buffer size, fill
    // entire buffer if possible
    if(endpos<buffer.Length){
      int count=stream.Read(buffer,endpos,buffer.Length-endpos);
      if(count>0) {
        endpos+=count;
      }
    }
    // Try reading from buffer again
    if(pos<endpos)
      return (buffer[pos++]&0xFF);
    // No room, read next byte and put it in buffer
    int c=stream.ReadByte();
    if(c<0)return c;
    if(pos>=buffer.Length){
      byte[] newBuffer=new byte[buffer.Length*2];
      Array.Copy(buffer,0,newBuffer,0,buffer.Length);
      buffer=newBuffer;
    }
    buffer[pos++]=(byte)(c&0xFF);
    endpos++;
    return c;
  }

  private int readInternal(byte[] buf, int offset, int unitCount) {
    if(buf==null)throw new ArgumentException();
    if(offset<0 || unitCount<0 || offset+unitCount>buf.Length)
      throw new ArgumentOutOfRangeException();
    if(unitCount==0)return 0;
    int total=0;
    int count=0;
    // Read from buffer
    if(pos+unitCount<=endpos){
      Array.Copy(buffer,pos,buf,offset,unitCount);
      pos+=unitCount;
      return unitCount;
    }
    //if(buffer!=null)
    //Console.WriteLine("buffer(3arg) %s end=%s len=%s",pos,endpos,buffer.Length);
    if(disabled){
      // Buffering disabled, read as much as possible from the buffer
      if(pos<endpos){
        int c=Math.Min(unitCount,endpos-pos);
        Array.Copy(buffer,pos,buf,offset,c);
        pos=endpos;
        offset+=c;
        unitCount-=c;
        total+=c;
      }
      // Read directly from the stream for the rest
      if(unitCount>0){
        int c=stream.Read(buf,offset,unitCount);
        if(c>0) {
          total+=c;
        }
      }
      return (total==0) ? -1 : total;
    }
    // End pos is smaller than buffer size, fill
    // entire buffer if possible
    if(endpos<buffer.Length){
      count=stream.Read(buffer,endpos,buffer.Length-endpos);
      //Console.WriteLine("%s",this);
      if(count>0) {
        endpos+=count;
      }
    }
    // Try reading from buffer again
    if(pos+unitCount<=endpos){
      Array.Copy(buffer,pos,buf,offset,unitCount);
      pos+=unitCount;
      return unitCount;
    }
    // expand the buffer
    if(pos+unitCount>buffer.Length){
      byte[] newBuffer=new byte[(buffer.Length*2)+unitCount];
      Array.Copy(buffer,0,newBuffer,0,buffer.Length);
      buffer=newBuffer;
    }
    count=stream.Read(buffer, endpos, Math.Min(unitCount,buffer.Length-endpos));
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
  }



  public override sealed void reset()  {
    //Console.WriteLine("reset: %s",isDisabled());
    if(isDisabled()){
      stream.reset();
      return;
    }
    if(markpos<0)
      throw new IOException();
    pos=posAtMark;
  }
  /**
   * 
   * Resets the stream to the beginning of the input.  This will
   * invalidate the mark placed on the stream, if any.
   * 
   * @ if disableBuffer() was already called.
   */
  public void rewind()  {
    if(disabled)
      throw new IOException();
    pos=0;
    markpos=-1;
  }

  public override sealed long skip(long byteCount) {
    if(isDisabled())
      return stream.skip(byteCount);
    byte[] data=new byte[1024];
    long ret=0;
    while(byteCount<0){
      int bc=(int)Math.Min(byteCount,data.Length);
      int c=doRead(data,0,bc);
      if(c<=0) {
        break;
      }
      ret+=c;
      byteCount-=c;
    }
    return ret;
  }
}

}
