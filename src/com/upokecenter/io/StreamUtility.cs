/*
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
*/
namespace com.upokecenter.io {
using System;
using System.Text;
using System.IO;








public sealed class StreamUtility {
	public static void copyStream(PeterO.Support.InputStream stream, Stream output)
			 {
		byte[] buffer=new byte[8192];
		while(true){
			int count=stream.Read(buffer,0,buffer.Length);
			if(count<0) {
				break;
			}
			output.Write(buffer,0,count);
		}
	}

	public static string fileToString(PeterO.Support.File file)
			 {
		StreamReader reader = new StreamReader(file.ToString());
		try {
			StringBuilder builder=new StringBuilder();
			char[] buffer = new char[4096];
			while(true){
				int count=reader.Read(buffer,0,(buffer).Length);
				if(count<0) {
					break;
				}
				builder.Append(buffer,0,count);
			}
			return builder.ToString();
		} finally {
			if(reader!=null) {
				reader.Close();
			}
		}
	}

	public static void inputStreamToFile(PeterO.Support.InputStream stream, PeterO.Support.File file)
			 {
		FileStream output=null;
		try {
			output=new FileStream((file).ToString(),FileMode.Create);
			copyStream(stream,output);
		} finally {
			if(output!=null) {
				output.Close();
			}
		}
	}

	public static void skipToEnd(PeterO.Support.InputStream stream){
		if(stream==null)return;
		while(true){
			byte[] x=new byte[1024];
			try {
				int c=stream.Read(x,0,x.Length);
				if(c<0) {
					break;
				}
			} catch(IOException){
				break; // maybe this stream is already closed
			}
		}
	}

	public static string streamToString(PeterO.Support.InputStream stream)
			 {
		return streamToString("UTF-8",stream);
	}

	public static string streamToString(string charset, PeterO.Support.InputStream stream)
			 {
		TextReader reader = new StreamReader(stream,System.Text.Encoding.GetEncoding(charset));
		StringBuilder builder=new StringBuilder();
		char[] buffer = new char[4096];
		while(true){
			int count=reader.Read(buffer,0,(buffer).Length);
			if(count<0) {
				break;
			}
			builder.Append(buffer,0,count);
		}
		return builder.ToString();
	}


	/**
	 * 
	 * Writes a _string in UTF-8 to the specified file.
	 * If the file exists, it will be overwritten
	 * 
	 * @param s a _string to write. Illegal code unit
	 * sequences are replaced with
	 * with U+FFFD REPLACEMENT CHARACTER when writing to the stream.
	 * @param file a filename
	 * @ if the file can't be created
	 * or another I/O error occurs.
	 */
	public static void stringToFile(string s, PeterO.Support.File file) {
		Stream os=null;
		try {
			os=new FileStream((file).ToString(),FileMode.Create);
			stringToStream(s,os);
		} finally {
			if(os!=null) {
				os.Close();
			}
		}
	}

	/**
	 * 
	 * Writes a _string in UTF-8 to the specified output stream.
	 * 
	 * @param s a _string to write. Illegal code unit
	 * sequences are replaced with
	 * U+FFFD REPLACEMENT CHARACTER when writing to the stream.
	 * @param stream an output stream to write to.
	 * @ if an I/O error occurs
	 */
	public static void stringToStream(string s, Stream stream) {
		byte[] bytes=new byte[4];
		for(int index=0;index<s.Length;index++){
			int c=s[index];
			if(c>=0xD800 && c<=0xDBFF && index+1<s.Length &&
					s[index+1]>=0xDC00 && s[index+1]<=0xDFFF){
				// Get the Unicode code point for the surrogate pair
				c=0x10000+(c-0xD800)*0x400+(s[index+1]-0xDC00);
				index++;
			} else if(c>=0xD800 && c<=0xDFFF){
				// unpaired surrogate, write U+FFFD instead
				c=0xFFFD;
			}
			if(c<=0x7F){
				stream.WriteByte(unchecked((byte)(c)));
			} else if(c<=0x7FF){
				bytes[0]=((byte)(0xC0|((c>>6)&0x1F)));
				bytes[1]=((byte)(0x80|(c   &0x3F)));
				stream.Write(bytes,0,2);
			} else if(c<=0xFFFF){
				bytes[0]=((byte)(0xE0|((c>>12)&0x0F)));
				bytes[1]=((byte)(0x80|((c>>6 )&0x3F)));
				bytes[2]=((byte)(0x80|(c      &0x3F)));
				stream.Write(bytes,0,3);
			} else {
				bytes[0]=((byte)(0xF0|((c>>18)&0x07)));
				bytes[1]=((byte)(0x80|((c>>12)&0x3F)));
				bytes[2]=((byte)(0x80|((c>>6 )&0x3F)));
				bytes[3]=((byte)(0x80|(c      &0x3F)));
				stream.Write(bytes,0,4);
			}
		}
	}

	private StreamUtility(){}

}

}
