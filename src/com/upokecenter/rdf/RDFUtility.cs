/*
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
*/


namespace com.upokecenter.rdf {
using System;
using System.Collections.Generic;

public sealed class RDFUtility {
	public static bool areIsomorphic(ISet<RDFTriple> graph1, ISet<RDFTriple> graph2){
		if(graph1==null)return graph2==null;
		if(graph1.Equals(graph2))return true;
		// Graphs must have the same size to be isomorphic
		if(graph1.Count!=graph2.Count)return false;
		foreach(var triple in graph1){
			// do a strict comparison
			if(triple.getSubject().getKind()!=RDFTerm.BLANK &&
					triple.getObject().getKind()!=RDFTerm.BLANK){
				if(!graph2.Contains(triple))
					return false;
			} else {
				// do a lax comparison
				bool found=false;
				foreach(var triple2 in graph2){
					if(laxEqual(triple,triple2)){
						found=true;
						break;
					}
				}
				if(!found)return false;
			}
		}
		return true;
	}

	/**
	 * A lax comparer of RDF triples which doesn't compare
	 * blank node labels
	 * 
	 * @param a
	 * @param b
	 * 
	 */
	private static bool laxEqual(RDFTriple a, RDFTriple b){
		if(a==null)return (b==null);
		if(a.Equals(b))return true;
		if(a.getSubject().getKind()!=b.getSubject().getKind())
			return false;
		if(a.getObject().getKind()!=b.getObject().getKind())
			return false;
		if(!a.getPredicate().Equals(b.getPredicate()))
			return false;
		if(a.getSubject().getKind()!=RDFTerm.BLANK){
			if(!a.getSubject().Equals(b.getSubject()))
				return false;
		}
		if(a.getObject().getKind()!=RDFTerm.BLANK){
			if(!a.getObject().Equals(b.getObject()))
				return false;
		}
		return true;
	}
	/**
	 * 
	 * Converts a set of RDF Triples to a JSON _object.  The _object
	 * contains all the subjects, each of which contains a dictionary
	 * of predicates for that subject, and each dictionary contains
	 * a list of objects for the subject and predicate.  The
	 * subject can either be a URI or a blank node (which starts
	 * with "_:".
	 * 
	 * @param triples
	 * 
	 */
  /*
	public static com.upokecenter.json.JSONObject RDFtoJSON(ISet<RDFTriple> triples){
		IDictionary<RDFTerm,IList<RDFTriple>> subjects=new PeterO.Support.LenientDictionary<RDFTerm,IList<RDFTriple>>();
		com.upokecenter.json.JSONObject rootJson=new com.upokecenter.json.JSONObject();
		foreach(var triple in triples){
			IList<RDFTriple> subjectList=subjects[triple.getSubject()];
			if(subjectList==null){
				subjectList=new List<RDFTriple>();
				subjects.Add(triple.getSubject(),subjectList);
			}
			subjectList.Add(triple);
		}
		foreach(var subject in subjects.Keys){
			com.upokecenter.json.JSONObject subjectJson=new com.upokecenter.json.JSONObject();
			IDictionary<RDFTerm,IList<RDFTerm>> predicates=new PeterO.Support.LenientDictionary<RDFTerm,IList<RDFTerm>>();
			foreach(var triple in triples){
				IList<RDFTerm> subjectList=predicates[triple.getPredicate()];
				if(subjectList==null){
					subjectList=new List<RDFTerm>();
					predicates.Add(triple.getPredicate(),subjectList);
				}
				subjectList.Add(triple.getObject());
			}
			foreach(var predicate in predicates.Keys){
				com.upokecenter.json.JSONArray valueArray=new com.upokecenter.json.JSONArray();
				foreach(var obj in predicates[predicate]){
					com.upokecenter.json.JSONObject valueJson=new com.upokecenter.json.JSONObject();
					if(obj.getKind()==RDFTerm.IRI){
						valueJson.put("type","uri");
						valueJson.put("value",obj.getValue());
					} else if(obj.getKind()==RDFTerm.LANGSTRING){
						valueJson.put("type","literal");
						valueJson.put("value",obj.getValue());
						valueJson.put("lang",obj.getTypeOrLanguage());
					} else if(obj.getKind()==RDFTerm.TYPEDSTRING){
						valueJson.put("type","literal");
						valueJson.put("value",obj.getValue());
						if(!obj.isOrdinaryString()) {
							valueJson.put("lang",obj.getTypeOrLanguage());
						}
					} else if(obj.getKind()==RDFTerm.BLANK){
						valueJson.put("type","bnode");
						valueJson.put("value",obj.getValue());
					}
					valueArray.put(valueJson);
				}
				subjectJson.put(predicate.getValue(),valueArray);
			}
			string subjKey=(subject.getKind()==RDFTerm.BLANK ? "_:" : "")+subject.getValue();
			rootJson.put(subjKey,subjectJson);
		}
		return rootJson;
	}
   */
	private RDFUtility(){}
}

}
