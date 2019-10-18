using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;


namespace Process_Export_Import
{
  
  [ServiceContract]
        
  
  public interface IProcess_Export_Import
  {
    
   
 
    ServiceCallResult Export_Process(Int32 processId);
    /*[OperationContract]
    ServiceCallResult Import_Process(string fileName);
   */
    
  }


  [DataContract]
    public class ServiceCallResult {
      [DataMember]
      public string Source {get;set;}
      [DataMember]
      public  int Code {get;set;}
      [DataMember]
      public string Description {get;set;}
      [DataMember]
      public string ExceptionContent {get;set;}
      [DataMember]
      public string InnerExceptionContent {get;set;}
                                            
  }
}
