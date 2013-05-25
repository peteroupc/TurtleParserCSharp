// Written by Peter Occil, 2013. In the public domain.
// Public domain dedication: http://creativecommons.org/publicdomain/zero/1.0/
namespace com.upokecenter.rdf {
using System;
using System.Text;

public sealed class RDFTerm {

	*
	 * Type value for a blank node.
	 */
	public static readonly int BLANK = 0; // type is blank node name, literal is blank

	/**
	 * Type value for an IRI (Internationalized Resource Identifier.)
	 */
	public static readonly int IRI = 1; // type is IRI, literal is blank

	/**
	 * Type value for a _string with a language tag.
	 */
	public static readonly int LANGSTRING = 2; // literal is given

	/**
	 * Type value for a piece of data serialized to a _string.
	 */
	public static readonly int TYPEDSTRING = 3; // type is IRI, literal is given

	private static void escapeBlankNode(string str, StringBuilder builder){
		int length=str.Length;
		string hex="0123456789ABCDEF";
		for(int i=0;i<length;i++){
			int c=str[i];
			if((c>='A' && c<='Z') || (c>='a' && c<='z') ||
					(c>0 && c>='0' && c<='9')){
				builder.Append((char)c);
			}
			else if(c>=0xD800 && c<=0xDBFF && i+1<length &&
					str[i+1]>=0xDC00 && str[i+1]<=0xDFFF){
				// Get the Unicode code point for the surrogate pair
				c=0x10000+(c-0xD800)*0x400+(str[i+1]-0xDC00);
				builder.Append("U00");
				builder.Append(hex[(c>>20)&15]);
				builder.Append(hex[(c>>16)&15]);
				builder.Append(hex[(c>>12)&15]);
				builder.Append(hex[(c>>8)&15]);
				builder.Append(hex[(c>>4)&15]);
				builder.Append(hex[(c)&15]);
				i++;
			}
			else {
				builder.Append("u");
				builder.Append(hex[(c>>12)&15]);
				builder.Append(hex[(c>>8)&15]);
				builder.Append(hex[(c>>4)&15]);
				builder.Append(hex[(c)&15]);
			}
		}
	}

	private static void escapeLanguageTag(string str, StringBuilder builder){
		int length=str.Length;
		bool hyphen=false;
		for(int i=0;i<length;i++){
			int c=str[i];
			if((c>='A' && c<='Z')){
				builder.Append((char)(c+0x20));
			} else if(c>='a' && c<='z'){
				builder.Append((char)c);
			} else if(hyphen && c>='0' && c<='9'){
				builder.Append((char)c);
			} else if(c=='-'){
				builder.Append((char)c);
				hyphen=true;
				if(i+1<length && str[i+1]=='-') {
					builder.Append('x');
				}
			} else {
				builder.Append('x');
			}
		}
	}
	private static void escapeString(string str,
			StringBuilder builder, bool uri){
		int length=str.Length;
		string hex="0123456789ABCDEF";
		for(int i=0;i<length;i++){
			int c=str[i];
			if(c==0x09){
				builder.Append("\\t");
			} else if(c==0x0a){
				builder.Append("\\n");
			} else if(c==0x0d){
				builder.Append("\\r");
			} else if(c==0x22){
				builder.Append("\\\"");
			} else if(c==0x5c){
				builder.Append("\\\\");
			} else if(uri && c=='>'){
				builder.Append("%3E");
			} else if(c>=0x20 && c<=0x7E){
				builder.Append((char)c);
			}
			else if(c>=0xD800 && c<=0xDBFF && i+1<length &&
					str[i+1]>=0xDC00 && str[i+1]<=0xDFFF){
				// Get the Unicode code point for the surrogate pair
				c=0x10000+(c-0xD800)*0x400+(str[i+1]-0xDC00);
				builder.Append("\\U00");
				builder.Append(hex[(c>>20)&15]);
				builder.Append(hex[(c>>16)&15]);
				builder.Append(hex[(c>>12)&15]);
				builder.Append(hex[(c>>8)&15]);
				builder.Append(hex[(c>>4)&15]);
				builder.Append(hex[(c)&15]);
				i++;
			}
			else {
				builder.Append("\\u");
				builder.Append(hex[(c>>12)&15]);
				builder.Append(hex[(c>>8)&15]);
				builder.Append(hex[(c>>4)&15]);
				builder.Append(hex[(c)&15]);
			}
		}
	}
	private string typeOrLanguage=null;
	private string value=null;
	private int kind;
	/**
	 * Predicate for RDF types.
	 */
	public static readonly RDFTerm A=
			fromIRI("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
	/**
	 * Predicate for the first object in a list.
	 */
	public static readonly RDFTerm FIRST=fromIRI(
			"http://www.w3.org/1999/02/22-rdf-syntax-ns#first");

	/**
	 * Object for nil, the end of a list, or an empty list.
	 */
	public static readonly RDFTerm NIL=fromIRI(
			"http://www.w3.org/1999/02/22-rdf-syntax-ns#nil");
	/**
	 * Predicate for the remaining objects in a list.
	 */
	public static readonly RDFTerm REST=fromIRI(
			"http://www.w3.org/1999/02/22-rdf-syntax-ns#rest");
	/**
	 * Object for false.
	 */
	public static readonly RDFTerm FALSE=fromTypedString(
			"false","http://www.w3.org/2001/XMLSchema#bool");

	/**
	 * Object for true.
	 */
	public static readonly RDFTerm TRUE=fromTypedString(
			"true","http://www.w3.org/2001/XMLSchema#bool");

	public static RDFTerm fromBlankNode(string name){
		if((name)==null)throw new ArgumentNullException("name");
		if((name).Length==0)throw new ArgumentException("name is empty.");
		RDFTerm ret=new RDFTerm();
		ret.kind=BLANK;
		ret.typeOrLanguage=null;
		ret.value=name;
		return ret;
	}

	public static RDFTerm fromIRI(string iri){
		if((iri)==null)throw new ArgumentNullException("iri");
		RDFTerm ret=new RDFTerm();
		ret.kind=IRI;
		ret.typeOrLanguage=null;
		ret.value=iri;
		return ret;
	}

	public static RDFTerm fromLangString(string str, string languageTag) {
		if((str)==null)throw new ArgumentNullException("str");
		if((languageTag)==null)throw new ArgumentNullException("languageTag");
		if((languageTag).Length==0)throw new ArgumentException("languageTag is empty.");
		RDFTerm ret=new RDFTerm();
		ret.kind=LANGSTRING;
		ret.typeOrLanguage=languageTag;
		ret.value=str;
		return ret;
	}
	public static RDFTerm fromTypedString(string str) {
		return fromTypedString(str,"http://www.w3.org/2001/XMLSchema#string");
	}
	public static RDFTerm fromTypedString(string str, string iri){
		if((str)==null)throw new ArgumentNullException("str");
		if((iri)==null)throw new ArgumentNullException("iri");
		if((iri).Length==0)throw new ArgumentException("iri is empty.");
		RDFTerm ret=new RDFTerm();
		ret.kind=TYPEDSTRING;
		ret.typeOrLanguage=iri;
		ret.value=str;
		return ret;
	}
	public override sealed bool Equals(object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (GetType() != obj.GetType())
			return false;
		RDFTerm other = (RDFTerm) obj;
		if (kind != other.kind)
			return false;
		if (typeOrLanguage == null) {
			if (other.typeOrLanguage != null)
				return false;
		} else if (!typeOrLanguage.Equals(other.typeOrLanguage))
			return false;
		if (value == null) {
			if (other.value != null)
				return false;
		} else if (!value.Equals(other.value))
			return false;
		return true;
	}
	public int getKind() {
		return kind;
	}
	/**
	 * Gets the language tag or data type for this RDF literal.
	 */
	public string getTypeOrLanguage() {
		return typeOrLanguage;
	}
	/**
	 * Gets the IRI, blank node identifier, or
	 * lexical form of an RDF literal.
	 * 
	 */
	public string getValue() {
		return value;
	}
	public override sealed int GetHashCode(){unchecked{
		 int prime = 31;
		int result = 1;
		result = prime * result + kind;
		result = prime * result
				+ ((typeOrLanguage == null) ? 0 : typeOrLanguage.GetHashCode());
		result = prime * result + ((value == null) ? 0 : value.GetHashCode());
		return result;
	}}
	public bool isBlank(){
		return kind==BLANK;
	}
	public bool isIRI(string str){
		return kind==IRI && str!=null && str.Equals(value);
	}
	public bool isOrdinaryString(){
		return kind==TYPEDSTRING && "http://www.w3.org/2001/XMLSchema#string".Equals(typeOrLanguage);
	}
	/**
	 * 
	 * Gets a _string representation of this RDF term
	 * in N-Triples format.  The _string will not end
	 * in a line break.
	 * 
	 
	public override sealed string ToString(){
		StringBuilder builder=null;
		if(this.kind==BLANK){
			builder=new StringBuilder();
			builder.Append("_:");
			escapeBlankNode(value,builder);
		} else if(this.kind==LANGSTRING){
			builder=new StringBuilder();
			builder.Append("\"");
			escapeString(value,builder,false);
			builder.Append("\"@");
			escapeLanguageTag(typeOrLanguage,builder);
		} else if(this.kind==TYPEDSTRING){
			builder=new StringBuilder();
			builder.Append("\"");
			escapeString(value,builder,false);
			builder.Append("\"");
			if(!"http://www.w3.org/2001/XMLSchema#string".Equals(typeOrLanguage)){
				builder.Append("^^<");
				escapeString(typeOrLanguage,builder,true);
				builder.Append(">");
			}
		} else if(this.kind==IRI){
			builder=new StringBuilder();
			builder.Append("<");
			escapeString(value,builder,true);
			builder.Append(">");
		} else
			return "<about:blank>";
		return builder.ToString();
	}
}
}
