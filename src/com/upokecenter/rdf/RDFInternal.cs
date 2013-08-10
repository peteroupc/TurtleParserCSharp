/*
Written in 2013 by Peter Occil.  
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/

If you like this, you should donate to Peter O.
at: http://upokecenter.com/d/
*/
namespace com.upokecenter.rdf {
using System;
using System.Globalization;
using System.Collections.Generic;





sealed class RDFInternal {
  /*
   *  Replaces certain blank nodes with blank nodes whose
   *  names meet the N-Triples requirements
   * @param triples A set of RDF triples
   * @param bnodeLabels A mapping of blank node names
   * already allocated.  This method will modify this
   * _object as needed to allocate new blank nodes.
   */
  //
  internal static void replaceBlankNodes(ISet<RDFTriple> triples,
      IDictionary<string,RDFTerm> bnodeLabels){
    if(bnodeLabels.Count==0)
      return;
    IDictionary<string,RDFTerm> newBlankNodes=new PeterO.Support.LenientDictionary<string,RDFTerm>();
    IList<RDFTriple[]> changedTriples=new List<RDFTriple[]>();
    int[] nodeindex=new int[]{0};
    foreach(var triple in triples){
      bool changed=false;
      RDFTerm subj=triple.getSubject();
      if(subj.getKind()==RDFTerm.BLANK){
        string oldname=subj.getValue();
        string newname=suggestBlankNodeName(oldname,nodeindex,bnodeLabels);
        if(!newname.Equals(oldname)){
          RDFTerm newNode=newBlankNodes[oldname];
          if(newNode==null){
            newNode=RDFTerm.fromBlankNode(newname);
            bnodeLabels.Add(newname, newNode);
            newBlankNodes.Add(oldname, newNode);
          }
          subj=newNode;
          changed=true;
        }
      }
      RDFTerm obj=triple.getObject();
      if(obj.getKind()==RDFTerm.BLANK){
        string oldname=obj.getValue();
        string newname=suggestBlankNodeName(oldname,nodeindex,bnodeLabels);
        if(!newname.Equals(oldname)){
          RDFTerm newNode=newBlankNodes[oldname];
          if(newNode==null){
            newNode=RDFTerm.fromBlankNode(newname);
            bnodeLabels.Add(newname, newNode);
            newBlankNodes.Add(oldname, newNode);
          }
          obj=newNode;
          changed=true;
        }
      }
      if(changed){
        RDFTriple[] newTriple=new RDFTriple[]{triple,
            new RDFTriple(subj,triple.getPredicate(),obj)
        };
        changedTriples.Add(newTriple);
      }
    }
    foreach(var triple in changedTriples){
      triples.Remove(triple[0]);
      triples.Add(triple[1]);
    }
  }



  private static string suggestBlankNodeName(
      string node, int[] nodeindex, IDictionary<string,RDFTerm> bnodeLabels){
    bool validnode=(node.Length>0);
    // Check if the blank node label is valid
    // under N-Triples
    for(int i=0;i<node.Length;i++){
      int c=node[i];
      if(i==0 && !((c>='A' && c<='Z') || (c>='a' && c<='z'))){
        validnode=false;
        break;
      }
      if(i>=0 && !((c>='A' && c<='Z') || (c>='0' && c<='9') ||
          (c>='a' && c<='z'))){
        validnode=false;
        break;
      }
    }
    if(validnode)return node;
    while(true){
      // Generate a new blank node label,
      // and ensure it's unique
      node="b"+Convert.ToString(nodeindex[0],CultureInfo.InvariantCulture);
      if(!bnodeLabels.ContainsKey(node))
        return node;
      nodeindex[0]++;
    }
  }
  private RDFInternal(){}
}

}
