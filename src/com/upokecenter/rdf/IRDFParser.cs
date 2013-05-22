// Written by Peter Occil, 2013. In the public domain.
// Public domain dedication: http://creativecommons.org/publicdomain/zero/1.0/
namespace com.upokecenter.rdf {
using System;
using System.IO;
using System.Collections.Generic;

public interface IRDFParser {
	 ISet<RDFTriple> parse() ;
}

}
