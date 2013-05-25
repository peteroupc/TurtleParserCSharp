
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
 */
namespace com.upokecenter.io {
using System;
using System.Text;
using System.Globalization;
using System.IO;


/**
 * 
 * Lightweight character input for UTF-8.
 * 
 * @author Peter
 *
 
public class Utf8CharacterInput : ICharacterInput {

	PeterO.Support.InputStream stream;

	public Utf8CharacterInput(PeterO.Support.InputStream stream){
		this.stream=stream;
	}

	public int read()  {
		int cp=0;
		int bytesSeen=0;
		int bytesNeeded=0;
		int lower=0x80;
		int upper=0xBF;
		while(true){
			int b=stream.ReadByte();
			if(b<0 && bytesNeeded!=0){
				bytesNeeded=0;
				throw new IOException("",new DecoderFallbackException());
			} else if(b<0)
				return -1;
			if(bytesNeeded==0){
				if(b<0x80)
					return b;
				else if(b>=0xc2 && b<=0xdf){
					stream.mark(4);
					bytesNeeded=1;
					cp=b-0xc0;
				} else if(b>=0xe0 && b<=0xef){
					stream.mark(4);
					lower=(b==0xe0) ? 0xa0 : 0x80;
					upper=(b==0xed) ? 0x9f : 0xbf;
					bytesNeeded=2;
					cp=b-0xe0;
				} else if(b>=0xf0 && b<=0xf4){
					stream.mark(4);
					lower=(b==0xf0) ? 0x90 : 0x80;
					upper=(b==0xf4) ? 0x8f : 0xbf;
					bytesNeeded=3;
					cp=b-0xf0;
				} else
					throw new IOException("",new DecoderFallbackException());
				cp<<=(6*bytesNeeded);
				continue;
			}
			if(b<lower || b>upper){
				cp=bytesNeeded=bytesSeen=0;
				lower=0x80;
				upper=0xbf;
				stream.reset();
				throw new IOException("",new DecoderFallbackException());
			}
			lower=0x80;
			upper=0xbf;
			bytesSeen++;
			cp+=(b-0x80)<<(6*(bytesNeeded-bytesSeen));
			stream.mark(4);
			if(bytesSeen!=bytesNeeded) {
				continue;
			}
			int ret=cp;
			cp=0;
			bytesSeen=0;
			bytesNeeded=0;
			return ret;
		}
	}

	public int read(int[] buf, int offset, int unitCount)
			 {
		if((buf)==null)throw new ArgumentNullException("buf");
		if((offset)<0)throw new ArgumentOutOfRangeException("offset"+" not greater or equal to "+"0"+" ("+Convert.ToString(offset,CultureInfo.InvariantCulture)+")");
		if((unitCount)<0)throw new ArgumentOutOfRangeException("unitCount"+" not greater or equal to "+"0"+" ("+Convert.ToString(unitCount,CultureInfo.InvariantCulture)+")");
		if((offset+unitCount)>buf.Length)throw new ArgumentOutOfRangeException("offset+unitCount"+" not less or equal to "+Convert.ToString(buf.Length,CultureInfo.InvariantCulture)+" ("+Convert.ToString(offset+unitCount,CultureInfo.InvariantCulture)+")");
		if(unitCount==0)return 0;
		for(int i=0;i<unitCount;i++){
			int c=read();
			if(c<0)
				return i==0 ? -1 : i;
			buf[offset++]=c;
		}
		return unitCount;
	}
}
}
