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

public interface IMarkableCharacterInput : ICharacterInput {

	 int getMarkPosition();

	 void moveBack(int count) ;

	 int setHardMark();

	 void setMarkPosition(int pos) ;

	 int setSoftMark();

}
}
