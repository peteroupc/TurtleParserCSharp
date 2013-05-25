
using System;
using System.Collections.Generic;
using System.IO;
using com.upokecenter.util;
using System.Net;

namespace PeterO.Support
{
	
	/// <summary>
	/// Description of Support.
	/// </summary>
	public class File
	{
		String path;
		public File(String path)
		{
			this.path=path;
		}
		public override string ToString()
		{
			return path;
		}

		public File(File path, String file)
		{
			this.path=Path.Combine(path.ToString(),file);
		}
		
		public File(String path, String file)
		{
			this.path=Path.Combine(path,file);
		}
		public bool delete(){
			System.IO.File.Delete(path);
			return !exists();
		}
		public String getName(){
			return System.IO.Path.GetFileName(path);
		}
		public bool exists(){
			return System.IO.File.Exists(path);
		}
		public bool isDirectory(){
			return (System.IO.File.GetAttributes(path)&FileAttributes.Directory)== FileAttributes.Directory;
		}
		public bool isFile(){
			return (System.IO.File.GetAttributes(path)&FileAttributes.Directory)== FileAttributes.Normal;
		}
		public long lastModified(){
			DateTime t=System.IO.File.GetLastWriteTimeUtc(path);
			long msec=t.Millisecond;
			long time=DateTimeUtility.toGmtDate(t.Year,t.Month,t.Day,t.Hour,t.Minute,t.Second)+msec;
			return time;
		}
		public long length(){
			return new FileInfo(path).Length;
		}
		public String toURI(){
			UriBuilder builder=new UriBuilder();
			builder.Scheme="file";
			builder.Path=path;
			return builder.Uri.ToString();
		}
		public File[] listFiles(){
			if(isFile())return new File[0];
			List<File> ret=new List<File>();
			foreach(var f in Directory.GetFiles(path)){
				ret.Add(new File(f));
			}
			foreach(var f in Directory.GetDirectories(path)){
				ret.Add(new File(f));
			}
			return ret.ToArray();
		}
	}

	public static class Collections {
		public static IList<T> UnmodifiableList<T>(IList<T> list){
			if(list.IsReadOnly)return list;
			return new System.Collections.ObjectModel.ReadOnlyCollection<T>(list);
		}
		public static IDictionary<TKey,TValue> UnmodifiableMap<TKey,TValue>(IDictionary<TKey,TValue> list){
			if(list.IsReadOnly)return list;
			return new ReadOnlyDictionary<TKey,TValue>(list);
		}
		public static T[] ToArray<T>(IEnumerable<T> enu){
			return ((enu as List<T>) ?? (new List<T>(enu))).ToArray();
		}
	}
	
	
	/**
	 * Dictionary that allows null keys and doesn't throw exceptions
	 * if keys are not found, both of which are HashMap behaviors.
	 */
	public sealed class LenientDictionary<TKey,TValue> : IDictionary<TKey,TValue> {
		private TValue nullValue;
		bool hasNull=false;
		private IDictionary<TKey,TValue> wrapped;
		
		public LenientDictionary(){
			this.wrapped=new Dictionary<TKey,TValue>();
		}
		
		public LenientDictionary(IDictionary<TKey,TValue> other){
			if(default(TKey)==null && other.ContainsKey(default(TKey))){
				// If dictionary contains null, add the values manually,
				// because the constructor will throw an exception
				// otherwise
				this.wrapped=new Dictionary<TKey,TValue>();
				foreach(var kvp in other){
					this.AddInternal(kvp.Key,kvp.Value);
				}
			} else {
				this.wrapped=new Dictionary<TKey,TValue>(other);
			}
		}

		public TValue this[TKey key] {
			get {
				if(Object.Equals(key,null) && hasNull && default(TKey)==null)
					return nullValue;
				TValue val;
				if(wrapped.TryGetValue(key,out val))
					return val;
				return default(TValue);
			}
			set {
				if(Object.Equals(key,null) && default(TKey)==null){
					hasNull=true;
					nullValue=value;
				}
				wrapped[key]=value;
			}
		}
		
		public ICollection<TKey> Keys {
			get {
				if(hasNull){
					var keys=new List<TKey>(wrapped.Keys);
					keys.Add(default(TKey));
					return keys;
				} else return wrapped.Keys;
			}
		}
		
		public ICollection<TValue> Values {
			get {
				if(hasNull){
					var keys=new List<TValue>(wrapped.Values);
					keys.Add(nullValue);
					return keys;
				} else return wrapped.Values;
			}
		}
		
		public int Count {
			get {
				return wrapped.Count+(hasNull ? 1 : 0);
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public bool ContainsKey(TKey key)
		{
			if(Object.Equals(key,null) && default(TKey)==null)
				return (hasNull);
			return wrapped.ContainsKey(key);
		}
		
		public void Add(TKey key, TValue value)
		{
			AddInternal(key,value);
		}
		
		private void AddInternal(TKey key, TValue value)
		{
			if(Object.Equals(key,null)){
				hasNull=true;
				nullValue=value;
			} else wrapped[key]=value;
		}
		public bool Remove(TKey key)
		{
			if(Object.Equals(key,null)){
				bool ret=hasNull;
				hasNull=false;
				nullValue=default(TValue);
				return ret;
			} else return wrapped.Remove(key);
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			if(Object.Equals(key,null)){
				value=(hasNull) ? nullValue : default(TValue);
				return hasNull;
			} else return wrapped.TryGetValue(key,out value);
		}
		
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key,item.Value);
		}
		
		public void Clear()
		{
			hasNull=true;
			nullValue=default(TValue);
			wrapped.Clear();
		}
		
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsKey(item.Key);
		}
		
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if(array!=null && arrayIndex<array.Length && hasNull){
				array[arrayIndex]=new KeyValuePair<TKey, TValue>(default(TKey),nullValue);
				arrayIndex++;
			}
			wrapped.CopyTo(array,arrayIndex);
		}
		
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}
		
		private IEnumerable<KeyValuePair<TKey, TValue>> Iterator(){
			if(hasNull){
				yield return new KeyValuePair<TKey, TValue>(default(TKey),nullValue);
			}
			foreach(var kvp in wrapped){
				yield return kvp;
			}
		}
		
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return Iterator().GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return Iterator().GetEnumerator();
		}
	}
	
	sealed class InputStreamWrapper : InputStream {
		
		Stream istr;
		public InputStreamWrapper(Stream istr){
			this.istr=istr;
		}		
		// Just ensures that read never returns a number
		// less than 0, for compatibility with StreamReader
		public override int Read(byte[] buffer, int offset, int count)
		{
			int ret=istr.Read(buffer,offset,count);
			if(ret<0)ret=0;
			return ret;
		}

		public override int ReadByte()
		{
			return istr.ReadByte();
		}
	}
	
	sealed class ReadOnlyDictionary<TKey,TValue> : IDictionary<TKey,TValue> {
		
		private IDictionary<TKey,TValue> wrapped;
		
		public ReadOnlyDictionary(IDictionary<TKey,TValue> wrapped){
			this.wrapped=wrapped;
		}
		
		public TValue this[TKey key] {
			get {
				return wrapped[key];
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		public ICollection<TKey> Keys {
			get {
				return wrapped.Keys;
			}
		}
		
		public ICollection<TValue> Values {
			get {
				return wrapped.Values;
			}
		}
		
		public int Count {
			get {
				return wrapped.Count;
			}
		}
		
		public bool IsReadOnly {
			get {
				return true;
			}
		}
		
		public bool ContainsKey(TKey key)
		{
			return wrapped.ContainsKey(key);
		}
		
		public void Add(TKey key, TValue value)
		{
			throw new NotSupportedException();
		}
		
		public bool Remove(TKey key)
		{
			throw new NotSupportedException();
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			return wrapped.TryGetValue(key,out value);
		}
		
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException();
		}
		
		public void Clear()
		{
			throw new NotSupportedException();
		}
		
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return wrapped.Contains(item);
		}
		
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			wrapped.CopyTo(array,arrayIndex);
		}
		
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotSupportedException();
		}
		
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return wrapped.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return wrapped.GetEnumerator();
		}
	}
	
	
	

	public sealed class WrappedInputStream : InputStream {
		
		Stream wrapped=null;
		public WrappedInputStream(Stream wrapped) {
			this.wrapped=wrapped;
		}

		public override sealed void Close() {
			wrapped.Close();
			base.Close();
		}

		public override sealed int ReadByte() {
			return wrapped.ReadByte();
		}

		public override sealed long skip(long byteCount) {
			byte[] data=new byte[1024];
			long ret=0;
			while(byteCount<0){
				int bc=(int)Math.Min(byteCount,data.Length);
				int c=Read(data,0,bc);
				if(c<=0) {
					break;
				}
				ret+=c;
				byteCount-=c;
			}
			return ret;
		}

		public override sealed int Read(byte[] buffer, int offset, int byteCount) {
			return wrapped.Read(buffer,offset,byteCount);
		}
	}

	public sealed class ByteArrayInputStream : InputStream {
		
		private byte[] buffer=null;
		private int pos=0;
		private int endpos=0;
		private long markpos=-1;
		private int posAtMark=0;
		private long marklimit=0;

		public ByteArrayInputStream(byte[] buffer) : this(buffer,0,buffer.Length) {
			
		}

		public ByteArrayInputStream(byte[] buffer, int index, int length) {
			if(buffer==null || index<0 || length<0 || index+length>buffer.Length)
				throw new ArgumentException();
			this.buffer=buffer;
			this.pos=index;
			this.endpos=index+length;
		}

		public override sealed void Close() {
		}

		public override sealed int available() {
			return endpos-pos;
		}

		public override sealed bool markSupported(){
			return true;
		}

		public override sealed void mark(int limit){
			if(limit<0)
				throw new ArgumentException();
			markpos=0;
			posAtMark=pos;
			marklimit=limit;
		}

		private int readInternal(byte[] buf, int offset, int unitCount) {
			if(buf==null)throw new ArgumentException();
			if(offset<0 || unitCount<0 || offset+unitCount>buf.Length)
				throw new ArgumentOutOfRangeException();
			if(unitCount==0)return 0;
			int total=Math.Min(unitCount,endpos-pos);
			if(total==0)return -1;
			Array.Copy(buffer,pos,buf,offset,total);
			pos+=total;
			return total;
		}

		private int readInternal()  {
			// Read from buffer
			if(pos<endpos)
				return (buffer[pos++]&0xFF);
			return -1;
		}

		public override sealed int ReadByte() {
			if(markpos<0)
				return readInternal();
			else {
				int c=readInternal();
				if(c>=0 && markpos>=0){
					markpos++;
					if(markpos>marklimit){
						marklimit=0;
						markpos=-1;
					}
				}
				return c;
			}
		}

		public override sealed long skip(long byteCount) {
			byte[] data=new byte[1024];
			long ret=0;
			while(byteCount<0){
				int bc=(int)Math.Min(byteCount,data.Length);
				int c=Read(data,0,bc);
				if(c<=0) {
					break;
				}
				ret+=c;
				byteCount-=c;
			}
			return ret;
		}

		public override sealed int Read(byte[] buffer, int offset, int byteCount) {
			if(markpos<0)
				return readInternal(buffer,offset,byteCount);
			else {
				int c=readInternal(buffer,offset,byteCount);
				if(c>0 && markpos>=0){
					markpos+=c;
					if(markpos>marklimit){
						marklimit=0;
						markpos=-1;
					}
				}
				return c;
			}
		}
		public override sealed void reset()  {
			if(markpos<0)
				throw new IOException();
			pos=posAtMark;
		}
	}
	
	public sealed class BufferedInputStream : InputStream {
		
		private byte[] buffer=null;
		private int pos=0;
		private int endpos=0;
		private bool closed=false;
		private long markpos=-1;
		private int posAtMark=0;
		private long marklimit=0;
		private Stream stream=null;

		public BufferedInputStream(Stream input) : this(input,8192) {
			
		}

		public BufferedInputStream(Stream input, int buffersize) {
			if(input==null)
				throw new ArgumentNullException();
			if(buffersize<0)
				throw new ArgumentException();
			this.buffer=new byte[buffersize];
			this.stream=input;
		}

		public override sealed void Close() {
			pos=0;
			endpos=0;
			this.stream.Close();
		}

		public override sealed int available() {
			return endpos-pos;
		}

		public override sealed bool markSupported(){
			return true;
		}

		public override sealed void mark(int limit){
			if(limit<0)
				throw new ArgumentException();
			markpos=0;
			posAtMark=pos;
			marklimit=limit;
		}

		private int readInternal(byte[] buf, int offset, int unitCount) {
			if(buf==null)throw new ArgumentException();
			if(offset<0 || unitCount<0 || offset+unitCount>buf.Length)
				throw new ArgumentOutOfRangeException();
			if(unitCount==0)return 0;
			int total=0;
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

		private int readInternal()  {
			// Read from buffer
			if(pos<endpos)
				return (buffer[pos++]&0xFF);
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
			buffer[pos++]=((byte)(c&0xFF));
			endpos++;
			return c;
		}

		public override sealed int ReadByte() {
			if(closed)
				throw new IOException();
			if(markpos<0)
				return readInternal();
			else {
				int c=readInternal();
				if(c>=0 && markpos>=0){
					markpos++;
					if(markpos>marklimit){
						marklimit=0;
						markpos=-1;
					}
				}
				return c;
			}
		}

		public override sealed long skip(long byteCount) {
			if(closed)
				throw new IOException();
			byte[] data=new byte[1024];
			long ret=0;
			while(byteCount<0){
				int bc=(int)Math.Min(byteCount,data.Length);
				int c=Read(data,0,bc);
				if(c<=0) {
					break;
				}
				ret+=c;
				byteCount-=c;
			}
			return ret;
		}

		public override sealed int Read(byte[] buffer, int offset, int byteCount) {
			if(closed)
				throw new IOException();
			if(markpos<0)
				return readInternal(buffer,offset,byteCount);
			else {
				int c=readInternal(buffer,offset,byteCount);
				if(c>0 && markpos>=0){
					markpos+=c;
					if(markpos>marklimit){
						marklimit=0;
						markpos=-1;
					}
				}
				return c;
			}
		}
		public override sealed void reset()  {
			if(markpos<0 || closed)
				throw new IOException();
			pos=posAtMark;
		}
	}

	public abstract class OutputStream : Stream {
		
		public override sealed void SetLength(long value)
		{
			throw new NotSupportedException();
		}
		
		public override sealed long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
		
		public override sealed int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
		
		public override sealed long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		public override sealed long Length {
			get {
				throw new NotSupportedException();
			}
		}
		
		public override void Flush()
		{
		}
		
		public override sealed bool CanWrite {
			get {
				return true;
			}
		}
		
		public override sealed bool CanSeek {
			get {
				return false;
			}
		}
		
		public override sealed bool CanRead {
			get {
				return false;
			}
		}
	}
	
	public abstract class InputStream : Stream {
		
		public virtual int available(){
			return 0;
		}
		
		public virtual void mark(int limit){
			throw new NotSupportedException();
		}
		
		public virtual void reset(){
			throw new NotSupportedException();
		}

		public virtual bool markSupported(){
			return false;
		}
		
		public virtual long skip(long count){
			return 0;
		}
		
		public override void Close(){
		}
		
		//------------------------------------------
		
		public sealed override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
		
		public sealed override void SetLength(long value)
		{
			throw new NotSupportedException();
		}
		
		public sealed override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
		
		public sealed override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}
		
		public sealed override long Length {
			get {
				throw new NotSupportedException();
			}
		}
		
		public sealed override void Flush()
		{
			throw new NotSupportedException();
		}
		
		public sealed override bool CanWrite {
			get {
				return false;
			}
		}
		
		public sealed override bool CanSeek {
			get {
				return false;
			}
		}
		
		public sealed override bool CanRead {
			get {
				return true;
			}
		}
	}
}
