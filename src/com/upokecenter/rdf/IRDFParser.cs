/*
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
*/
namespace com.upokecenter.rdf {
using System;
using System.IO;
using System.Collections.Generic;

public interface IRDFParser {
   ISet<RDFTriple> parse() ;
}

}
