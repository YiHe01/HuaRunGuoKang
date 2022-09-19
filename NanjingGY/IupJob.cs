
using AppInfo;
using DbHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RomensInterfaceExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static NanjingGY.jsonClass;

namespace NanjingGY
{



    public class IupJob : RomensInterfaceExtension.IUpJob
    {
        private static object Singleton_Lock = new object();

        public bool ExecJob(RomensInterfaceExtension.JobPara paras, System.Data.DataSet dsData, out string errorMsg)
        {
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string jobCode = paras.planTaskCode;//任务编号 10001
            string orgCode = paras.orgCode;//1000  //区分业主
            string planTaskCode = paras.jobCode;//计划编号 100  上传下载
            //SourceDataAdapter sourAdapter = SourceDataAdapter.GetInstance("1000");


            //获取账号 账密 密钥等
            Appinfo appinfo = new Appinfo();
            string platformKey = appinfo.platformKey;
            string platformSerect = appinfo.platformSerect;

            string companyCode = appinfo.companyCode;
            string appkey = appinfo.appkey;
            string YourAppSecret = appinfo.YourAppSecret;



            errorMsg = "";
            //判断dsData是否为空
            if (dsData.Tables.Count == 0)
            {
                errorMsg = "";
                return true;
            }

            ClientDataAdapter dataAdapter;
            SourceDataAdapter sourAdapter;
            string V1_URL, DBType, V2_URL;
            lock (Singleton_Lock)
            {
                sourAdapter = SourceDataAdapter.GetInstance("1000");
                dataAdapter = ClientDataAdapter.GetInstance();

                //url链接
                string v_url = $"Select  V1_URL,V2_URL From SaveUrl ";
                DataTable url_table = dataAdapter.GetDataTable(v_url);
                V1_URL = url_table.Rows[0]["V1_URL"].ToString();
                V2_URL = url_table.Rows[0]["V2_URL"].ToString();

                //数据库类型
                DBType = $"Select DBType From OrgSetting where code ='1000'";
                DataTable paramTable = dataAdapter.GetDataTable(DBType);
                DBType = paramTable.Rows[0]["DBType"].ToString();
            }


            #region jobName = "101" 商品信息  goods_select
            if ("101".Equals(planTaskCode))
            {
                LogSQL("接收商品信息接口", "接收商品信息");
                try
                {
                    string bus_type = "goods_select";
                    string ser_number = "GK" ;

                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送商品信息接口", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送商品信息接口", "url:" + V1_URL + strMethod);
                    LogSQL("推送商品信息接口", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root Info = JsonConvert.DeserializeObject<jsonClass.Root>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem> deliverys = Info.data;
                        foreach (jsonClass.DataItem Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPGOODS_INF  A ");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  SET GOODS_ID ={Root.GOODS_ID  },GOODS_MAIN={Root.GOODS_MAIN  } ,GOODSSYS_ID={Root.GOODSSYS_ID  } ,GOODS_NAME={Root.GOODS_NAME  } ,CLASS_CODE={Root.CLASS_CODE  } ,UNIT_ID={Root.UNIT_ID  } , ");
                            stbSql.AppendLine($"        UNIT_NAME={Root.UNIT_NAME  } ,GOODS_SPEC={Root.GOODS_SPEC  } ,MAKER_ID={Root.MAKER_ID  } ,MAKER_NAME={Root.MAKER_NAME  } ,GOODS_TYPE={Root.GOODS_TYPE  } ,QUERY_ID={Root.QUERY_ID  } , ");
                            stbSql.AppendLine($"        LICENSE={Root.LICENSE  } ,RATIO={Root.RATIO  } ,RATIO1={Root.RATIO1  } ,ISDELETED={Root.ISDELETED  } ,ORG_ID={Root.MAKER_NAME  } ,OWNER_ID={Root.OWNER_ID  } ,CREATE_ORG={Root.CREATE_ORG  } ,CREATE_ORGSEQ={Root.CREATE_ORGSEQ  } ,UPD_DATE= {Root.UPD_DATE  }, ");
                            stbSql.AppendLine($"        UPD_MODE={Root.UPD_MODE  } ,UPD_STATE={Root.UPD_STATE  } ,OPT_DATE={Root.OPT_DATE  } ,ROW_ID= {Root.ROW_ID  }  ");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (GOODS_ID ,GOODS_MAIN ,GOODSSYS_ID ,GOODS_NAME ,CLASS_CODE ,UNIT_ID ,UNIT_NAME ,GOODS_SPEC ,MAKER_ID ,  ");
                            stbSql.AppendLine($"   MAKER_NAME ,GOODS_TYPE ,QUERY_ID ,LICENSE ,RATIO ,RATIO1 ,ISDELETED ,ORG_ID ,OWNER_ID ,CREATE_ORG , ");
                            stbSql.AppendLine($"   CREATE_ORGSEQ ,UPD_DATE ,UPD_MODE ,UPD_STATE ,OPT_DATE ,ROW_ID )   ");
                            stbSql.AppendLine("  values ( ");

                            stbSql.AppendLine($" '{Root.GOODS_ID    }'");
                            stbSql.AppendLine($" '{Root.GOODS_MAIN    }'");
                            stbSql.AppendLine($" '{Root.GOODSSYS_ID    }'");
                            stbSql.AppendLine($" '{Root.GOODS_NAME    }'");
                            stbSql.AppendLine($" '{Root.CLASS_CODE    }'");
                            stbSql.AppendLine($" '{Root.UNIT_ID    }'");
                            stbSql.AppendLine($" '{Root.UNIT_NAME    }'");
                            stbSql.AppendLine($" '{Root.GOODS_SPEC    }'");
                            stbSql.AppendLine($" '{Root.MAKER_ID    }'");
                            stbSql.AppendLine($" '{Root.MAKER_NAME    }'");
                            stbSql.AppendLine($" '{Root.GOODS_TYPE    }'");
                            stbSql.AppendLine($" '{Root.QUERY_ID    }'");
                            stbSql.AppendLine($" '{Root.LICENSE    }'");
                            stbSql.AppendLine($" '{Root.RATIO    }'");
                            stbSql.AppendLine($" '{Root.RATIO1    }'");
                            stbSql.AppendLine($" '{Root.ISDELETED    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.OPT_DATE    }'");
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");

                            stbSql.AppendLine("  ); ");
                        }

                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "102" 客户信息
            if ("102".Equals(planTaskCode))
            {
                LogSQL("接收客户信息接口", "接收客户信息");
                try
                {
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送客户信息接口", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送客户信息接口", "url:" + V1_URL + strMethod);
                    LogSQL("推送客户信息接口", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root Info = JsonConvert.DeserializeObject<jsonClass.Root>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem> deliverys = Info.data;
                        foreach (jsonClass.DataItem Root in deliverys)
                        {

                            stbSql.AppendLine($" MERGE INTO  ERPCLIENT  A ");
                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  CLIENT_ID ={Root.CLIENT_ID  } ,CLIENTDIS_ID ={Root.CLIENTDIS_ID  } ,CLASS_ID ={Root.CLASS_ID  } ,NAME ={Root.NAME  }  ,LEVELER ={Root.LEVELER  }  ,ADDRESS ={Root.ADDRESS  }  ,CONTACT ={Root.CONTACT  }  ,QUERYID ={Root.QUERYID  }  ,DELIVER_ADDR ={Root.DELIVER_ADDR  }  , ");
                            stbSql.AppendLine($"        ORG_ID ={Root.ORG_ID  }  ,OWNER_ID ={Root.OWNER_ID  }  ,CREATE_ORG  ={Root.CREATE_ORG  } ,CREATE_ORGSEQ  ={Root.CREATE_ORGSEQ  } ,UPD_STATE  ={Root.UPD_STATE  } ,UPD_MODE ={Root.UPD_MODE  }  ,UPD_DATE ={Root.UPD_DATE  }  ,OPT_DATE ={Root.OPT_DATE  }  ,ROW_ID ={Root.ROW_ID  }  ,BILL_FLAG ={Root.BILL_FLAG  }  ");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT ( CLIENT_ID ,CLIENTDIS_ID ,CLASS_ID ,NAME ,LEVELER ,ADDRESS ,CONTACT ,QUERYID ,DELIVER_ADDR , ");
                            stbSql.AppendLine($"   ORG_ID, OWNER_ID, CREATE_ORG, CREATE_ORGSEQ, UPD_STATE, UPD_MODE, UPD_DATE, OPT_DATE, ROW_ID, BILL_FLAG  ");
                            stbSql.AppendLine("  values ( ");

                            stbSql.AppendLine($" '{Root.ROW_ID    }'");
                            stbSql.AppendLine($" '{Root.CLIENT_ID    }'");
                            stbSql.AppendLine($" '{Root.CLIENTDIS_ID    }'");
                            stbSql.AppendLine($" '{Root.CLASS_ID    }'");
                            stbSql.AppendLine($" '{Root.NAME    }'");
                            stbSql.AppendLine($" '{Root.LEVELER    }'");
                            stbSql.AppendLine($" '{Root.ADDRESS    }'");
                            stbSql.AppendLine($" '{Root.CONTACT    }'");
                            stbSql.AppendLine($" '{Root.QUERYID    }'");
                            stbSql.AppendLine($" '{Root.DELIVER_ADDR    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.OPT_DATE    }'");
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");
                            stbSql.AppendLine($" '{Root.BILL_FLAG    }'");



                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "103" 供应商信息
            if ("103".Equals(planTaskCode))
            {
                LogSQL("接收供应商信息接口", "接收供应商信息");
                try
                {
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送供应商信息接口", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送供应商信息接口", "url:" + V1_URL + strMethod);
                    LogSQL("推送供应商信息接口", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root Info = JsonConvert.DeserializeObject<jsonClass.Root>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {

                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem> deliverys = Info.data;
                        foreach (jsonClass.DataItem Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPSUPPLIER_INF  A ");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  SET  SUPPLIER_ID ={Root.SUPPLIER_ID  } ,SUPPLIERDIS_ID ={Root.SUPPLIERDIS_ID  }   ,SUPPLIER_NAME ={Root.SUPPLIER_NAME  }   , ");
                            stbSql.AppendLine($"        QUERY_ID ={Root.QUERY_ID  }   ,ORG_ID ={Root.ORG_ID  }   ,OWNER_ID ={Root.OWNER_ID  }   ,CREATE_ORG ={Root.CREATE_ORG  }   , ");
                            stbSql.AppendLine($"        CREATE_ORGSEQ ={Root.CREATE_ORGSEQ  }   ,OPT_DATE ={Root.OPT_DATE  }   ,UPD_DATE ={Root.UPD_DATE  }   ,UPD_MODE ={Root.UPD_MODE  }   ,UPD_STATE ={Root.UPD_STATE  }   ,ROW_ID ={Root.ROW_ID  }  ");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (SUPPLIER_ID ,SUPPLIERDIS_ID,SUPPLIER_NAME,QUERY_ID,ORG_ID,OWNER_ID,CREATE_ORG,CREATE_ORGSEQ,OPT_DATE,UPD_DATE,UPD_MODE,UPD_STATE,ROW_ID   ");
                            stbSql.AppendLine("  values ( ");

                            stbSql.AppendLine($" '{Root.SUPPLIER_ID   }'");
                            stbSql.AppendLine($" '{Root.SUPPLIERDIS_ID   }'");
                            stbSql.AppendLine($" '{Root.SUPPLIER_NAME   }'");
                            stbSql.AppendLine($" '{Root.QUERY_ID   }'");
                            stbSql.AppendLine($" '{Root.ORG_ID   }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID   }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG   }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ   }'");
                            stbSql.AppendLine($" '{Root.OPT_DATE   }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE   }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE   }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE   }'");
                            stbSql.AppendLine($" '{Root.ROW_ID   }'");

                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "104" 送货地址信息
            if ("104".Equals(planTaskCode))
            {
                LogSQL("接收送货地址信息接口", "接收送货地址信息");
                try
                {
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送送货地址信息接口", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送送货地址信息接口", "url:" + V1_URL + strMethod);
                    LogSQL("推送送货地址信息接口", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root Info = JsonConvert.DeserializeObject<jsonClass.Root>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem> deliverys = Info.data;
                        foreach (jsonClass.DataItem Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPCLIENT_ADDRESS  A ");
                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  SET ADD_ID={Root.ADD_ID  } ,CLIENT_ID={Root.CLIENT_ID  } ,CLIENTSYS_ID={Root.CLIENTSYS_ID  } ,ADDRESS={Root.ADDRESS  } ,ORG_ID={Root.ORG_ID  } ,OWNER_ID={Root.OWNER_ID  } ,CREATE_ORG={Root.CREATE_ORG  } , ");
                            stbSql.AppendLine($"       CREATE_ORGSEQ={Root.CREATE_ORGSEQ  } ,UPD_DATE={Root.UPD_DATE  } ,UPD_STATE={Root.UPD_STATE  } ,UPD_MODE={Root.UPD_MODE  } ,REMARK={Root.REMARK  } , ");
                            stbSql.AppendLine($"        ROW_ID={Root.ROW_ID  } ,OWNER_ADDRESS={Root.OWNER_ADDRESS  } ,OWNER_NAME={Root.OWNER_NAME  } ,PROV_CODE={Root.PROV_CODE  } ,CITY_CODE={Root.CITY_CODE  } ,AREA_COD={Root.AREA_COD  }");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (ADD_ID ,CLIENT_ID ,CLIENTSYS_ID ,ADDRESS ,ORG_ID ,OWNER_ID ,CREATE_ORG ,  ");
                            stbSql.AppendLine($"      CREATE_ORGSEQ ,UPD_DATE ,UPD_STATE ,UPD_MODE ,REMARK , ");
                            stbSql.AppendLine($"     ROW_ID ,OWNER_ADDRESS ,OWNER_NAME ,PROV_CODE ,CITY_CODE ,AREA_COD )   ");
                            stbSql.AppendLine("  values ( ");

                            stbSql.AppendLine($" '{Root.ADD_ID    }'");
                            stbSql.AppendLine($" '{Root.CLIENT_ID    }'");
                            stbSql.AppendLine($" '{Root.CLIENTSYS_ID    }'");
                            stbSql.AppendLine($" '{Root.ADDRESS    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.REMARK    }'");
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ADDRESS    }'");
                            stbSql.AppendLine($" '{Root.OWNER_NAME    }'");
                            stbSql.AppendLine($" '{Root.PROV_CODE    }'");
                            stbSql.AppendLine($" '{Root.CITY_CODE    }'");
                            stbSql.AppendLine($" '{Root.AREA_COD    }'");


                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "105" 商品分类
            if ("105".Equals(planTaskCode))
            {
                LogSQL("接收商品分类接口", "接收商品分类");
                try
                {
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送商品分类接口", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送商品分类接口", "url:" + V1_URL + strMethod);
                    LogSQL("推送商品分类接口", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root Info = JsonConvert.DeserializeObject<jsonClass.Root>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem> deliverys = Info.data;
                        foreach (jsonClass.DataItem Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPGOODS_SORT  A ");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  SORT_ID ={Root.SORT_ID  },SORT_FLAG ={Root.SORT_FLAG  } ,GOODS_ID ={Root.GOODS_ID  } ,SORT_NAME ={Root.SORT_NAME  } ,CREATE_DATE ={Root.CREATE_DATE  } ,CREATE_MAN ={Root.CREATE_MAN  } ,ORG_ID ={Root.ORG_ID  } , ");
                            stbSql.AppendLine($"        OWNER_ID ={Root.OWNER_ID  } ,CREATE_ORG ={Root.CREATE_ORG  } ,CREATE_ORGSEQ ={Root.CREATE_ORGSEQ  } ,UPD_DATE ={Root.UPD_DATE  } ,UPD_MODE ={Root.UPD_MODE  } ,UPDSTAT ={Root.UPDSTAT  } ,ROW_ID ={Root.ROW_ID  } ");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (SORT_ID ,SORT_FLAG ,GOODS_ID ,SORT_NAME ,CREATE_DATE ,CREATE_MAN ,ORG_ID ,  ");
                            stbSql.AppendLine($"         OWNER_ID ,CREATE_ORG ,CREATE_ORGSEQ ,UPD_DATE ,UPD_MODE ,UPDSTAT ,ROW_ID ");
                            stbSql.AppendLine("  values ( ");

                            stbSql.AppendLine($" '{Root.AREA_COD    }'");
                            stbSql.AppendLine($" '{Root.SORT_ID    }'");
                            stbSql.AppendLine($" '{Root.SORT_FLAG    }'");
                            stbSql.AppendLine($" '{Root.GOODS_ID    }'");
                            stbSql.AppendLine($" '{Root.SORT_NAME    }'");
                            stbSql.AppendLine($" '{Root.CREATE_DATE    }'");
                            stbSql.AppendLine($" '{Root.CREATE_MAN    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.UPDSTAT    }'");
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");


                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "106" 证件照信息
            if ("106".Equals(planTaskCode))
            {
                LogSQL("接收证件照信息接口", "接收证件照信息");
                try
                {
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送证件照信息接口", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送证件照信息接口", "url:" + V1_URL + strMethod);
                    LogSQL("推送证件照信息接口", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root Info = JsonConvert.DeserializeObject<jsonClass.Root>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem> deliverys = Info.data;
                        foreach (jsonClass.DataItem Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPLICENSE_INF  A");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  SET ROW_ID ={Root.ROW_ID  } ,LICENSE_STYLE ={Root.LICENSE_STYLE  } ,UNIT_ID  ={Root.UNIT_ID  } ,LICENSE_ID  ={Root.LICENSE_ID  } ,LICENSE_DESCRIPTION  ={Root.LICENSE_DESCRIPTION  } ,RELEASE_DATE  ={Root.RELEASE_DATE  } , ");
                            stbSql.AppendLine($"        VALID_DATE  ={Root.VALID_DATE  } ,CHECK_STYLE  ={Root.CHECK_STYLE  } ,CREATE_MAN  ={Root.CREATE_MAN  } ,CREATE_DATE  ={Root.CREATE_DATE  } ,CREATE_ORG  ={Root.CREATE_ORG  } ,CREATE_ORGSEQ  ={Root.CREATE_ORGSEQ  } , ");
                            stbSql.AppendLine($"        ORG_ID  ={Root.ORG_ID  } ,OWNER_ID  ={Root.OWNER_ID  } ,UPD_MODE  ={Root.UPD_MODE  } ,UPD_DATE  ={Root.UPD_DATE  } ,UPD_STATE  ={Root.UPD_STATE  } ,OPT_DATE  ={Root.OPT_DATE  }  ");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (ROW_ID ,LICENSE_STYLE ,UNIT_ID ,LICENSE_ID ,LICENSE_DESCRIPTION ,RELEASE_DATE ,  ");
                            stbSql.AppendLine($"           VALID_DATE ,CHECK_STYLE ,CREATE_MAN ,CREATE_DATE ,CREATE_ORG ,CREATE_ORGSEQ , ");
                            stbSql.AppendLine($"            ORG_ID ,OWNER_ID ,UPD_MODE ,UPD_DATE ,UPD_STATE ,OPT_DATE   ");
                            stbSql.AppendLine("  values ( ");


                            stbSql.AppendLine($" '{Root.ROW_ID    }'");
                            stbSql.AppendLine($" '{Root.LICENSE_STYLE    }'");
                            stbSql.AppendLine($" '{Root.UNIT_ID    }'");
                            stbSql.AppendLine($" '{Root.LICENSE_ID    }'");
                            stbSql.AppendLine($" '{Root.LICENSE_DESCRIPTION    }'");
                            stbSql.AppendLine($" '{Root.RELEASE_DATE    }'");
                            stbSql.AppendLine($" '{Root.VALID_DATE    }'");
                            stbSql.AppendLine($" '{Root.CHECK_STYLE    }'");
                            stbSql.AppendLine($" '{Root.CREATE_MAN    }'");
                            stbSql.AppendLine($" '{Root.CREATE_DATE    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.OPT_DATE    }'");


                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "201" 出库订单
            if ("201".Equals(planTaskCode))
            {
                LogSQL("接收出库订单接口", "接收出库订单");
                try
                {
                    //参数1 
                    string bus_type = "preturn_select";
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送出库订单", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送出库订单", "url:" + V1_URL + strMethod);
                    LogSQL("推送出库订单", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root_201 Info = JsonConvert.DeserializeObject<jsonClass.Root_201>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem_201> deliverys = Info.data;
                        foreach (jsonClass.DataItem_201 Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPSALEPROOF  A");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE ROW_ID ='{Root.ROW_ID} ' , PROOF_ID = '{Root.PROOF_ID}', SRC_PROOF_ID = '{Root.SRC_PROOF_ID}' , SRC_LINE_ID = '{Root.SRC_LINE_ID}' , SALE_TYPE = '{Root.SALE_TYPE}' , PROOF_DATE = '{Root.PROOF_DATE}' ,   ");
                            stbSql.AppendLine($"        CUSTOMER_ID = '{Root.CUSTOMER_ID}' , CUSTOMERSYS_ID = '{Root.CUSTOMERSYS_ID}' , GOODS_ID = '{Root.GOODS_ID}' , UPD_DATE = '{Root.UPD_DATE}' ,  ");
                            stbSql.AppendLine($"        GOODSSYS_ID = '{Root.GOODSSYS_ID}' , BATCH_NO = '{Root.BATCH_NO}' , VALID_DATE = '{Root.VALID_DATE}' , PRODUCT_DATE = '{Root.PRODUCT_DATE}' , QUANTITY = '{Root.PRODUCT_DATE}' ,");
                            stbSql.AppendLine($"        PRICE = '{Root.PRICE}' , GOODS_AMOUNT = '{Root.GOODS_AMOUNT}' , WS_PRICE = '{Root.WS_PRICE}' , RT_PRICE = '{Root.RT_PRICE}' ,  UPD_MODE = '{Root.UPD_MODE}'  ,");
                            stbSql.AppendLine($"        BUY_PRICE = '{Root.BUY_PRICE}' , WAREHOST_ID = '{Root.WAREHOST_ID}' , WAREHOUSE_TYPE = '{Root.WAREHOUSE_TYPE}' , PROOF_HUMAN = '{Root.PROOF_HUMAN}' , ");
                            stbSql.AppendLine($"        BILL_LEVEL = '{Root.BILL_LEVEL}' , COMPANY_ID = '{Root.COMPANY_ID}' , DEPART_ID2 = '{Root.DEPART_ID2}' , MONOMAR = '{Root.MONOMAR}' ,  UPD_STATE = '{Root.UPD_STATE}' , ");
                            stbSql.AppendLine($"        YAOJIAN_FLAG = '{Root.YAOJIAN_FLAG}' , ORG_ID = '{Root.ORG_ID}' , OWNER_ID = '{Root.OWNER_ID}' , CREATE_ORG = '{Root.CREATE_ORG}' , CREATE_ORGSEQ = '{Root.CREATE_ORGSEQ}' , MEMO_FILE1 = '{Root.MEMO_FILE1}' ");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (PROOF_ID , SRC_PROOF_ID , SRC_LINE_ID , SALE_TYPE , PROOF_DATE , CUSTOMER_ID , CUSTOMERSYS_ID , GOODS_ID , ");
                            stbSql.AppendLine($"           GOODSSYS_ID , BATCH_NO , VALID_DATE , PRODUCT_DATE , QUANTITY , PRICE , GOODS_AMOUNT , WS_PRICE ,  ");
                            stbSql.AppendLine($"           RT_PRICE , BUY_PRICE , WAREHOST_ID , WAREHOUSE_TYPE , PROOF_HUMAN , BILL_LEVEL , COMPANY_ID , DEPART_ID2 , MONOMAR ,    ");
                            stbSql.AppendLine($"           YAOJIAN_FLAG , ORG_ID , OWNER_ID , CREATE_ORG , CREATE_ORGSEQ , UPD_DATE , UPD_MODE , UPD_STATE , MEMO_FILE1  ");
                            stbSql.AppendLine("  values ( ");
                             
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");
                            stbSql.AppendLine($" '{Root.PROOF_ID    }'");
                            stbSql.AppendLine($" '{Root.SRC_PROOF_ID    }'");
                            stbSql.AppendLine($" '{Root.SRC_LINE_ID    }'");
                            stbSql.AppendLine($" '{Root.SALE_TYPE    }'");
                            stbSql.AppendLine($" '{Root.PROOF_DATE    }'");
                            stbSql.AppendLine($" '{Root.CUSTOMER_ID    }'");
                            stbSql.AppendLine($" '{Root.CUSTOMERSYS_ID    }'");
                            stbSql.AppendLine($" '{Root.GOODS_ID    }'");
                            stbSql.AppendLine($" '{Root.GOODSSYS_ID    }'");
                            stbSql.AppendLine($" '{Root.BATCH_NO    }'");
                            stbSql.AppendLine($" '{Root.VALID_DATE    }'");
                            stbSql.AppendLine($" '{Root.PRODUCT_DATE    }'");
                            stbSql.AppendLine($" '{Root.QUANTITY    }'");
                            stbSql.AppendLine($" '{Root.PRICE    }'");
                            stbSql.AppendLine($" '{Root.GOODS_AMOUNT    }'");
                            stbSql.AppendLine($" '{Root.WS_PRICE    }'");
                            stbSql.AppendLine($" '{Root.RT_PRICE    }'");
                            stbSql.AppendLine($" '{Root.BUY_PRICE    }'");
                            stbSql.AppendLine($" '{Root.WAREHOST_ID    }'");
                            stbSql.AppendLine($" '{Root.WAREHOUSE_TYPE    }'");
                            stbSql.AppendLine($" '{Root.PROOF_HUMAN    }'");
                            stbSql.AppendLine($" '{Root.BILL_LEVEL    }'");
                            stbSql.AppendLine($" '{Root.COMPANY_ID    }'");
                            stbSql.AppendLine($" '{Root.DEPART_ID2    }'");
                            stbSql.AppendLine($" '{Root.MONOMAR    }'");
                            stbSql.AppendLine($" '{Root.YAOJIAN_FLAG    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.MEMO_FILE1    }'");


                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "202" 入库订单
            if ("202".Equals(planTaskCode))
            {
                LogSQL("接收入库订单接口", "接收入库订单");
                try
                {
                    //参数1 
                    string bus_type = "putin_select";
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送入库订单", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送入库订单", "url:" + V1_URL + strMethod);
                    LogSQL("推送入库订单", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root_202 Info = JsonConvert.DeserializeObject<jsonClass.Root_202>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem_202> deliverys = Info.data;
                        foreach (jsonClass.DataItem_202 Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPBUYPLAN  A");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE SRC_PROOF_ID ='{Root.SRC_PROOF_ID}', SRC_LINE_ID ='{Root.SRC_LINE_ID}', SUPPLIER_ID ='{Root.SUPPLIER_ID}', GOODS_ID ='{Root.GOODS_ID}', BATCH_NO ='{Root.BATCH_NO}', VALID_DATE ='{Root.VALID_DATE}',    ");
                            stbSql.AppendLine($"        PRODUCT_DATE ='{Root.PRODUCT_DATE}', BUY_PRICE ='{Root.BUY_PRICE}', QUANTITY ='{Root.QUANTITY}', PIECE ='{Root.PIECE}', AMOUNT ='{Root.AMOUNT}', PROOF_MAN ='{Root.PROOF_MAN}', PROOF_DATE ='{Root.PROOF_DATE}',   ");
                            stbSql.AppendLine($"       ORDER_FLAG ='{Root.ORDER_FLAG}', RECEIVE_TYPE ='{Root.RECEIVE_TYPE}', STOCKCALGROUP_ID ='{Root.STOCKCALGROUP_ID}', CREATE_MAN ='{Root.CREATE_MAN}', CREATE_DATE ='{Root.CREATE_DATE}', ORG_ID ='{Root.ORG_ID}', ");
                            stbSql.AppendLine($"       OWNER_ID ='{Root.OWNER_ID}', CREATE_ORG ='{Root.CREATE_ORG}', CREATE_ORGSEQ ='{Root.CREATE_ORGSEQ}', UPD_DATE ='{Root.UPD_DATE}', UPD_STATE ='{Root.UPD_STATE}', UPD_MODE ='{Root.UPD_MODE}', OPT_DATE ='{Root.OPT_DATE}', ROW_ID ='{Root.ROW_ID}'");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (SRC_PROOF_ID , SRC_LINE_ID , SUPPLIER_ID , GOODS_ID , BATCH_NO , VALID_DATE ,  ");
                            stbSql.AppendLine($"           PRODUCT_DATE , BUY_PRICE , QUANTITY , PIECE , AMOUNT , PROOF_MAN , PROOF_DATE ,  ");
                            stbSql.AppendLine($"           ORDER_FLAG , RECEIVE_TYPE , STOCKCALGROUP_ID , CREATE_MAN , CREATE_DATE , ORG_ID ,    ");
                            stbSql.AppendLine($"           OWNER_ID , CREATE_ORG , CREATE_ORGSEQ , UPD_DATE , UPD_STATE , UPD_MODE , OPT_DATE , ROW_ID   ");
                            stbSql.AppendLine("  values ( ");

                            
                            stbSql.AppendLine($" '{Root.SRC_PROOF_ID    }'");
                            stbSql.AppendLine($" '{Root.SRC_LINE_ID    }'");
                            stbSql.AppendLine($" '{Root.SUPPLIER_ID    }'");
                            stbSql.AppendLine($" '{Root.GOODS_ID    }'");
                            stbSql.AppendLine($" '{Root.BATCH_NO    }'");
                            stbSql.AppendLine($" '{Root.VALID_DATE    }'");
                            stbSql.AppendLine($" '{Root.PRODUCT_DATE    }'");
                            stbSql.AppendLine($" '{Root.BUY_PRICE    }'");
                            stbSql.AppendLine($" '{Root.QUANTITY    }'");
                            stbSql.AppendLine($" '{Root.PIECE    }'");
                            stbSql.AppendLine($" '{Root.AMOUNT    }'");
                            stbSql.AppendLine($" '{Root.PROOF_MAN    }'");
                            stbSql.AppendLine($" '{Root.PROOF_DATE    }'");
                            stbSql.AppendLine($" '{Root.ORDER_FLAG    }'");
                            stbSql.AppendLine($" '{Root.RECEIVE_TYPE    }'");
                            stbSql.AppendLine($" '{Root.STOCKCALGROUP_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_MAN    }'");
                            stbSql.AppendLine($" '{Root.CREATE_DATE    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.OPT_DATE    }'");
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");




                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion

            #region jobName = "204" 移库订单数据
            if ("204".Equals(planTaskCode))
            {
                LogSQL("接收移库订单接口", "接收移库订单");
                try
                {
                    //参数1 
                    string bus_type = "putin_select";
                    string strMethod = "/api/addThirdGoods";
                    DataTable dt = dsData.Tables[0];

                    string SignStr = string.Empty;
                    string SignStrMD5 = string.Empty;
                    string Sql = $"Select ColumnName,ColumnValue From DataColumnRelation where  FType = '{planTaskCode}'";

                    ClientDataAdapter dataAdapters = ClientDataAdapter.GetInstance();
                    DataTable table = dataAdapters.GetDataTable(Sql);

                    Dictionary<string, string> dicTable = new Dictionary<string, string>();

                    foreach (DataRow item in table.Rows)
                    {
                        string strErpName = item["ColumnName"].ToString().ToUpper();
                        string strXmlName = item["ColumnValue"].ToString();

                        dicTable.Add(strErpName, strXmlName);

                    }


                    //获取当前时间,
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //JSON报文
                    JObject jsonArrData = GetJsonObject_10001(dt, dicTable, platformKey, platformSerect);

                    //查看报文格式
                    LogSQL("推送移库订单", "报文格式:" + jsonArrData.ToString());
                    string errMsg = Helper.wmsRespos(jsonArrData, V1_URL, strMethod);
                    LogSQL("推送移库订单", "url:" + V1_URL + strMethod);
                    LogSQL("推送移库订单", "返回报文:" + errMsg);  //3788856
                    jsonClass.Root_204 Info = JsonConvert.DeserializeObject<jsonClass.Root_204>(errMsg);
                    StringBuilder stbSql = new StringBuilder();

                    if (Info.data.Count < 1)
                    {
                        return true;
                    }

                    try
                    {
                        stbSql.AppendLine("  begin  ");
                        List<jsonClass.DataItem_204> deliverys = Info.data;
                        foreach (jsonClass.DataItem_204 Root in deliverys)
                        {
                            stbSql.AppendLine($" MERGE INTO  ERPWAREHOUSEMOVE_TASK  A");

                            stbSql.AppendLine($" USING ( SELECT '{Root.ROW_ID}' ROW_ID FROM DUAL ) B ");
                            stbSql.AppendLine($" ON (A.ROW_ID = B.ROW_ID )");
                            stbSql.AppendLine($" when matched then ");
                            stbSql.AppendLine($"   UPDATE  SRC_PROOF_ID ='{Root.SRC_PROOF_ID}' ,SRC_LINE_ID  ='{Root.SRC_LINE_ID}',MOVE_TYPE  ='{Root.MOVE_TYPE}',SRC_STOCKCALGROUP_ID  ='{Root.SRC_STOCKCALGROUP_ID}',DEST_STOCKCALGROUP_ID ='{Root.DEST_STOCKCALGROUP_ID}' ,   ");
                            stbSql.AppendLine($"        GOODS_ID  ='{Root.GOODS_ID}',VALID_DATE  ='{Root.VALID_DATE}',PRODUCT_DATE ='{Root.PRODUCT_DATE}' ,BATCH_NO ='{Root.BATCH_NO}' ,CREATE_MAN  ='{Root.CREATE_MAN}',CREATE_DATE  ='{Root.CREATE_DATE}',  ");
                            stbSql.AppendLine($"        SRC_WAREHOUSE_TYPE  ='{Root.SRC_WAREHOUSE_TYPE}',QUANTITY ='{Root.QUANTITY}' ,DEST_WAREHOUSE_TYPE  ='{Root.DEST_WAREHOUSE_TYPE}',STATE  ='{Root.STATE}',ORG_ID  ='{Root.ORG_ID}',OWNER_ID  ='{Root.OWNER_ID}', ");
                            stbSql.AppendLine($"        CREATE_ORG ='{Root.CREATE_ORG}' ,CREATE_ORGSEQ  ='{Root.CREATE_ORGSEQ}',UPD_STATE ='{Root.UPD_STATE}' ,UPD_MODE ='{Root.UPD_MODE}' ,UPD_DATE  ='{Root.UPD_DATE}',OPT_DATE ='{Root.OPT_DATE}' ,ROW_ID ='{Root.ROW_ID}'");
                            stbSql.AppendLine($" when not matched then ");
                            stbSql.AppendLine($"   INSERT (SRC_PROOF_ID ,SRC_LINE_ID ,MOVE_TYPE ,SRC_STOCKCALGROUP_ID ,DEST_STOCKCALGROUP_ID , ");
                            stbSql.AppendLine($"           GOODS_ID ,VALID_DATE ,PRODUCT_DATE ,BATCH_NO ,CREATE_MAN ,CREATE_DATE ,  ");
                            stbSql.AppendLine($"           SRC_WAREHOUSE_TYPE ,QUANTITY ,DEST_WAREHOUSE_TYPE ,STATE ,ORG_ID ,OWNER_ID ,   ");
                            stbSql.AppendLine($"           CREATE_ORG ,CREATE_ORGSEQ ,UPD_STATE ,UPD_MODE ,UPD_DATE ,OPT_DATE ,ROW_ID  ");
                            stbSql.AppendLine("  values ( ");

                            stbSql.AppendLine($" '{Root.ROW_ID    }'");
                            stbSql.AppendLine($" '{Root.SRC_PROOF_ID    }'");
                            stbSql.AppendLine($" '{Root.SRC_LINE_ID    }'");
                            stbSql.AppendLine($" '{Root.MOVE_TYPE    }'");
                            stbSql.AppendLine($" '{Root.SRC_STOCKCALGROUP_ID    }'");
                            stbSql.AppendLine($" '{Root.DEST_STOCKCALGROUP_ID    }'");
                            stbSql.AppendLine($" '{Root.GOODS_ID    }'");
                            stbSql.AppendLine($" '{Root.VALID_DATE    }'");
                            stbSql.AppendLine($" '{Root.PRODUCT_DATE    }'");
                            stbSql.AppendLine($" '{Root.BATCH_NO    }'");
                            stbSql.AppendLine($" '{Root.CREATE_MAN    }'");
                            stbSql.AppendLine($" '{Root.CREATE_DATE    }'");
                            stbSql.AppendLine($" '{Root.SRC_WAREHOUSE_TYPE    }'");
                            stbSql.AppendLine($" '{Root.QUANTITY    }'");
                            stbSql.AppendLine($" '{Root.DEST_WAREHOUSE_TYPE    }'");
                            stbSql.AppendLine($" '{Root.STATE    }'");
                            stbSql.AppendLine($" '{Root.ORG_ID    }'");
                            stbSql.AppendLine($" '{Root.OWNER_ID    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORG    }'");
                            stbSql.AppendLine($" '{Root.CREATE_ORGSEQ    }'");
                            stbSql.AppendLine($" '{Root.UPD_STATE    }'");
                            stbSql.AppendLine($" '{Root.UPD_MODE    }'");
                            stbSql.AppendLine($" '{Root.UPD_DATE    }'");
                            stbSql.AppendLine($" '{Root.OPT_DATE    }'");
                            stbSql.AppendLine($" '{Root.ROW_ID    }'");



                            stbSql.AppendLine("  ); ");
                        }
                        stbSql.AppendLine("  end;");
                    }
                    catch (Exception)
                    {

                        throw;
                    }




                }
                catch (Exception)
                {

                    throw;
                }
            }

            #endregion





            errorMsg = "未找到对应单据类型";
            return true;
        }






        /// <summary>
        /// 日志文件记录
        /// </summary>
        /// <param name="msg">写入信息</param>
        public static void Log(string content)
        {
            try
            {
                string filename = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string filePath = AppDomain.CurrentDomain.BaseDirectory + filename;
                FileInfo file = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + filename);
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString());
                sb.Append(" ");
                sb.Append(content);
                FileMode fm = new FileMode();
                if (!file.Exists)
                {
                    fm = FileMode.Create;
                }
                else
                {
                    fm = FileMode.Append;
                }
                using (FileStream fs = new FileStream(filePath, fm, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /// <summary>
        /// 日志文件记录
        /// </summary>
        /// <param name="msg">写入信息</param>
        public static void logError(string content)
        {
            try
            {
                string filename = "BILLERROR" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string filePath = AppDomain.CurrentDomain.BaseDirectory + filename;
                FileInfo file = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + filename);
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString());
                sb.Append(" ");
                sb.Append(content);
                FileMode fm = new FileMode();
                if (!file.Exists)
                {
                    fm = FileMode.Create;
                }
                else
                {
                    fm = FileMode.Append;
                }
                using (FileStream fs = new FileStream(filePath, fm, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /// <summary>
        /// 拼接基础资料json串  有递进
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dicYqTable"></param>
        /// <param name="dicTable"></param>
        /// <param name="dicIntTable"></param>
        /// <returns></returns>
        private JObject GetJObject(DataTable dt, Dictionary<string, string> dicYqTable, Dictionary<string, string> dicTable, Dictionary<string, string> dicIntTable, string pointStr)
        {
            JObject jObject = new JObject();
            JArray jArray = new JArray();
            JObject returnJobj = new JObject();
            string sjc = string.Empty;
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sjc = Helper.GetTimeStamp(string.Empty);
                //验签
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加 string
                    if (ifExis(dicYqTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = string.Empty;
                        if (dicYqTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("success".Equals(xmlName))
                            {
                                SignStr += xmlName + $"={ dt.Rows[i][j].ToString()}&";
                            }
                            else SignStr += xmlName + $"=\"{ dt.Rows[i][j].ToString()}\"&";
                        }
                    }
                    //如果该列被允许添加 int
                    if (ifExis(dicIntTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = string.Empty;
                        if (dicIntTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("timestamp".Equals(xmlName))
                            {
                                if (dt.Rows[i][j].ToString() != "0") sjc = Helper.GetTimeStamp(dt.Rows[i][j].ToString());
                                SignStr += xmlName + "=" + sjc + "&";
                            }
                            else
                            {
                                SignStr += xmlName + $"={ dt.Rows[i][j].ToString()}&";
                            }
                        }
                    }
                }
                SignStrMD5 = getSign(SignStr);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加到xml中
                    if (ifExis(dicTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("platformKey".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("sign".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, SignStrMD5);
                            }
                            else if ("success".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, Convert.ToBoolean(dt.Rows[i][j].ToString()));
                            }
                            else
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }

                        }
                    }
                }
                jArray.Add(jObject);
            }
            returnJobj.Add(pointStr, jArray);
            returnJobj.Add("timestamp", Convert.ToInt32(sjc));
            return returnJobj;
        }
        /// <summary>
        /// 拼接基础资料json串  有递进 timestamp不是时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dicYqTable"></param>
        /// <param name="dicTable"></param>
        /// <param name="dicIntTable"></param>
        /// <returns></returns>
        private JObject GetStringJObject(DataTable dt, Dictionary<string, string> dicYqTable, Dictionary<string, string> dicTable, Dictionary<string, string> dicIntTable, string pointStr)
        {
            JObject jObject = new JObject();
            JArray jArray = new JArray();
            JObject returnJobj = new JObject();
            string sjc = string.Empty;
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sjc = Helper.GetTimeStamp(string.Empty);
                //验签
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加 string
                    if (ifExis(dicYqTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = string.Empty;
                        if (dicYqTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("timestamp".Equals(xmlName))
                            {
                                if (dt.Rows[i][j].ToString() != "") sjc = Helper.GetTimeStamp(dt.Rows[i][j].ToString(), false);
                                SignStr += xmlName + $"=\"{ sjc}\"&";
                            }
                            else if ("success".Equals(xmlName))
                            {
                                SignStr += xmlName + $"={ dt.Rows[i][j].ToString()}&";
                            }
                            else SignStr += xmlName + $"=\"{ dt.Rows[i][j].ToString()}\"&";
                        }
                    }
                }
                //Log("SignStr:" + SignStr);
                SignStrMD5 = getSign(SignStr);

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加到xml中
                    if (ifExis(dicTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("platformKey".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("platformCode".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("timestamp".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }

                        }
                    }
                    //如果该列被允许添加 int
                    if (ifExis(dicIntTable, dt.Columns[j].ColumnName.ToUpper()) && (!string.IsNullOrWhiteSpace(dt.Rows[i][j].ToString())))
                    {
                        string xmlName = "";
                        if (dicIntTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("timestamp".Equals(xmlName))
                            {
                                if (dt.Rows[i][j].ToString() != "0")
                                    sjc = Helper.GetTimeStamp(dt.Rows[i][j].ToString());
                                jObject.Add(xmlName, Convert.ToInt32(sjc));
                            }
                            else
                            {
                                try
                                {
                                    jObject.Add(xmlName, Convert.ToInt32(dt.Rows[i][j].ToString()));
                                }
                                catch (Exception)
                                {
                                    jObject.Add(xmlName, decimal.Parse(dt.Rows[i][j].ToString()));
                                }
                            }

                        }
                    }
                }
                jArray.Add(jObject);
            }
            returnJobj.Add(pointStr, jArray);
            returnJobj.Add("sign", SignStrMD5);
            return returnJobj;
        }

        /// <summary>
        /// 拼接基础资料json串
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dicYqTable">验签</param>
        /// <param name="dicTable">字符类型</param>
        /// <param name="dicIntTable">int类型</param>
        /// <returns></returns>
        private JArray GetJsonObject(DataTable datatble, Dictionary<string, string> dicYqTable, Dictionary<string, string> dicTable, Dictionary<string, string> dicIntTable)
        {
            JObject jObject = new JObject();
            JArray jArray = new JArray();
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;
            string sjc = string.Empty;

            for (int i = 0; i < datatble.Rows.Count; i++)
            {
                string goodsId = string.Empty;
                sjc = Helper.GetTimeStamp(string.Empty);

                //验签
                for (int j = 0; j < datatble.Columns.Count; j++)
                {
                    if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) continue;
                    //如果该列被允许添加 string
                    if (ifExis(dicYqTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = string.Empty;
                        if (dicYqTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("goodsId".Equals(xmlName))
                            {
                                goodsId = datatble.Rows[i][j].ToString();
                            }
                            else
                            {
                                SignStr += xmlName + $"=\"{ datatble.Rows[i][j].ToString()}\"&";
                            }
                        }
                    }
                    //如果该列被允许添加 int
                    if (ifExis(dicIntTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = string.Empty;
                        if (dicIntTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("timestamp".Equals(xmlName))
                            {
                                SignStr += xmlName + "=" + sjc + "&";
                            }
                            else
                            {
                                SignStr += xmlName + $"={ datatble.Rows[i][j].ToString()}&";
                            }
                        }
                    }
                }
                if (SignStr.Contains("isAk=0")) SignStr = SignStr.Replace("&goodsName", $"&goodsId={goodsId}&goodsName");
                SignStrMD5 = getSign(SignStr);

                for (int j = 0; j < datatble.Columns.Count; j++)
                {
                    if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) continue;
                    //如果该列被允许添加 string
                    if (ifExis(dicTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("sign".Equals(xmlName))
                            {
                                jObject.Add(xmlName, SignStrMD5);
                            }
                            else
                            {
                                jObject.Add(xmlName, datatble.Rows[i][j].ToString());
                            }

                        }
                        if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) jObject.Remove(xmlName);
                    }

                    //如果该列被允许添加 int
                    if (ifExis(dicIntTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicIntTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("timestamp".Equals(xmlName))
                            {
                                if (datatble.Rows[i][j].ToString() != "0")
                                    sjc = Helper.GetTimeStamp(datatble.Rows[i][j].ToString());
                                jObject.Add(xmlName, Convert.ToInt32(sjc));
                            }
                            else
                            {
                                try
                                {
                                    jObject.Add(xmlName, Convert.ToInt32(datatble.Rows[i][j].ToString()));
                                }
                                catch (Exception)
                                {
                                    jObject.Add(xmlName, decimal.Parse(datatble.Rows[i][j].ToString()));
                                }
                            }

                        }
                    }
                }
                //isAk
                string finger = string.Empty;
                try
                {
                    finger = jObject["isAk"].ToString();
                }
                catch (Exception)
                {
                    finger = string.Empty;
                }

                if (finger == "1") jObject.Remove("goodsId");
                jArray.Add(jObject);
            }
            return jArray;
        }


        private JObject GetJsonObject_10005(DataTable datatble, Dictionary<string, string> dicTable, string platformKey, string platformSerect, int page)
        {
            JObject jObject = new JObject();
            JObject jObjects = new JObject();
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;
            int sjc = GetTimeStamptoint();


            for (int i = 0; i < datatble.Rows.Count; i++)
            {
                string goodsId = string.Empty;
                sjc = GetTimeStamptoint();

                SignStrMD5 = getSign11(sjc.ToString());

                for (int j = 0; j < datatble.Columns.Count; j++)
                {

                    if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) continue;
                    //如果该列被允许添加 string
                    if (ifExis(dicTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("sign".Equals(xmlName))
                            {
                                jObject.Add(xmlName, SignStrMD5);
                            }

                            else if ("page".Equals(xmlName))
                            {

                                jObject.Add(xmlName, page++);

                            }
                            else if ("limit".Equals(xmlName))
                            {

                                jObject.Add(xmlName, Convert.ToInt32(datatble.Rows[i][j].ToString()));

                            }

                            else
                            {
                                jObject.Add(xmlName, datatble.Rows[i][j].ToString());
                            }

                        }
                        if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) jObject.Remove(xmlName);
                    }

                }
                //isAk
                string finger = string.Empty;
                try
                {
                    finger = jObject["isAk"].ToString();
                }
                catch (Exception)
                {
                    finger = string.Empty;
                }

                if (finger == "1") jObject.Remove("goodsId");
                //jObject.Add("page", page);
                jObject.Add("timestamp", sjc);
                jObject.Add("platformKey", platformKey);
                jObject.Add("platformSerect", platformSerect);

            }
            return jObject;
        }


        private JObject GetJsonObject_10001(DataTable datatble, Dictionary<string, string> dicTable, string platformKey, string platformSerect)
        {
            JObject jObject = new JObject();
            JObject jObjects = new JObject();
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;
            int sjc = GetTimeStamptoint();


            for (int i = 0; i < datatble.Rows.Count; i++)
            {
                string goodsId = string.Empty;
                sjc = GetTimeStamptoint();

                SignStrMD5 = getSign11(sjc.ToString());

                for (int j = 0; j < datatble.Columns.Count; j++)
                {

                    if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) continue;
                    //如果该列被允许添加 string
                    if (ifExis(dicTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("sign".Equals(xmlName))
                            {
                                jObject.Add(xmlName, SignStrMD5);
                            }

                            else if ("goodsId".Equals(xmlName))
                            {
                                goodsId = datatble.Rows[i][j].ToString();
                                jObject.Add(xmlName, goodsId = datatble.Rows[i][j].ToString());
                            }

                            else if ("limit".Equals(xmlName))
                            {

                                jObject.Add(xmlName, Convert.ToInt32(datatble.Rows[i][j].ToString()));

                            }

                            else
                            {
                                jObject.Add(xmlName, datatble.Rows[i][j].ToString());
                            }

                        }
                        if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) jObject.Remove(xmlName);
                    }

                }
                //isAk
                if (SignStr.Contains("isAk=0")) SignStr = SignStr.Replace("&goodsName", $"&goodsId={goodsId}&goodsName");

                //isAk
                string finger = string.Empty;
                try
                {
                    finger = jObject["isAk"].ToString();
                }
                catch (Exception)
                {
                    finger = string.Empty;
                }

                if (finger == "1") jObject.Remove("goodsId");


                jObject.Add("timestamp", sjc);


            }
            return jObject;
        }

        private JObject GetStringJObject_10002(DataTable dt, Dictionary<string, string> dicTable, string pointStr, string platformSerect)
        {
            JObject jObject = new JObject();
            JArray jArray = new JArray();
            JObject returnJobj = new JObject();
            int sjc = 0;
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;

            // platformKey="YTDYF"&platformSerect="YTDYF"&timestamp=1624930272116&

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sjc = GetTimeStamptoint();
                SignStrMD5 = getSign11(sjc.ToString());
                //验签
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加到xml中
                    if (ifExis(dicTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("platformKey".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("platformSerect".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("timestamp".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, sjc);
                            }
                            else if ("sign".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, SignStrMD5);
                            }
                            else if ("num".Equals(xmlName))
                            {
                                jObject.Add(xmlName, Convert.ToInt32(dt.Rows[i][j].ToString()));
                            }
                            else
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }

                        }


                    }

                }

                jArray.Add(jObject);
            }
            returnJobj.Add(pointStr, jArray);

            return returnJobj;
        }
        private JObject GetJsonObject_10004(DataTable dt, Dictionary<string, string> dicTable)
        {
            JObject jObject = new JObject();

            //数组格式
            JArray jArray = new JArray();
            //数组明细组成
            JObject jObjectDet = new JObject();

            int sjc = GetTimeStamptoint();
            //int sjc = GetTimeStamp() ;
            string SignStrMD5 = getSign11(sjc.ToString());

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jObjectDet = new JObject();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加到xml中
                    if (ifExis(dicTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("platformKey".Equals(xmlName))
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("platformSerect".Equals(xmlName))
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("timestamp".Equals(xmlName))
                            {
                                jObject.Add(xmlName, sjc);
                            }
                            else if ("sign".Equals(xmlName))
                            {

                                jObject.Add(xmlName, SignStrMD5);


                            }
                            else
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }

                        }
                    }
                }


            }

            //如果sign
            if (jObject.Property("sign") != null)
            {

                return jObject;
            }
            else
            {
                jObject.Add("sign", SignStrMD5);
            }

            return jObject;
        }
        private JObject GetStringJObject_10006(DataTable dt, Dictionary<string, string> dicTable, string pointStr, string platformSerect)
        {
            JObject jObject = new JObject();
            JArray jArray = new JArray();
            JObject returnJobj = new JObject();
            int sjc = 0;
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;

            // platformKey="YTDYF"&platformSerect="YTDYF"&timestamp=1624930272116&

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sjc = GetTimeStamptoint();
                SignStrMD5 = getSign11(sjc.ToString());
                //验签
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    //如果该列被允许添加到xml中
                    if (ifExis(dicTable, dt.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(dt.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("platformKey".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("platformSerect".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, dt.Rows[i][j].ToString());
                            }
                            else if ("timestamp".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, sjc);
                            }
                            else if ("sign".Equals(xmlName))
                            {
                                returnJobj.Add(xmlName, SignStrMD5);
                            }
                            else
                            {
                                jObject.Add(xmlName, dt.Rows[i][j].ToString());
                            }

                        }
                    }

                }

                jArray.Add(jObject);
            }
            returnJobj.Add(pointStr, jArray);

            return returnJobj;
        }
        private JObject GetJsonObject_20010(int pageNo, string jmsign, string appkey, string timestamp, DataTable datatble, Dictionary<string, string> dicTable)
        {
            JObject jObject = new JObject();
            JObject jObjects = new JObject();
            string SignStr = string.Empty;
            string SignStrMD5 = string.Empty;
            int sjc = GetTimeStamptoint();


            for (int i = 0; i < datatble.Rows.Count; i++)
            {
                string goodsId = string.Empty;
                sjc = GetTimeStamptoint();

                SignStrMD5 = getSign11(sjc.ToString());

                for (int j = 0; j < datatble.Columns.Count; j++)
                {

                    if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) continue;
                    //如果该列被允许添加 string
                    if (ifExis(dicTable, datatble.Columns[j].ColumnName.ToUpper()))
                    {
                        string xmlName = "";
                        if (dicTable.TryGetValue(datatble.Columns[j].ColumnName.ToString().ToUpper(), out xmlName))
                        {
                            if ("sign".Equals(xmlName))
                            {
                                jObject.Add(xmlName, SignStrMD5);
                            }
                            else if ("limit".Equals(xmlName))
                            {

                                jObject.Add(xmlName, Convert.ToInt32(datatble.Rows[i][j].ToString()));

                            }

                            else
                            {
                                jObject.Add(xmlName, datatble.Rows[i][j].ToString());
                            }

                        }
                        if (string.IsNullOrWhiteSpace(datatble.Rows[i][j].ToString())) jObject.Remove(xmlName);
                    }

                }


                jObject.Add("version", "v2.0");
                jObject.Add("pageNo", pageNo);
                jObject.Add("sign", jmsign);
                jObject.Add("pageSize", 100);
                jObject.Add("appKey", appkey);
                jObject.Add("timestamp", timestamp);

                //var json = new Dictionary<string, object>();
                //json.Add("companyCode", companyCode);
                //json.Add("version", "v2.0");
                //json.Add("pageNo", pageNo);
                //json.Add("sign", jmsign);
                //json.Add("updateTime", updateTime);
                //json.Add("pageSize", 100);
                //json.Add("appKey", appkey);
                //json.Add("timestamp", timestamps);

                //string jsontostring = JsonConvert.SerializeObject(json);
            }
            return jObject;
        }



        /// <summary>
        /// 判断传过来的数据源的列是否生成到Json中
        /// </summary>
        /// <param name="dicTable"></param>
        /// <param name="xxStr"></param>
        /// <returns></returns>
        public bool ifExis(Dictionary<string, string> dicTable, string xxStr)
        {
            bool boolFlg = false;
            foreach (var temp in dicTable)
            {
                if (temp.Key.Equals(xxStr))
                {
                    boolFlg = true;
                }
            }
            return boolFlg;
        }
        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string getSign(string str)
        {

            string returnStr = string.Empty;
            string YqStr = string.Empty;
            string[] data = str.Split('&');
            string[] arr = data.OrderBy(p => p).ToArray();
            foreach (string s in arr)
            {
                if (!string.IsNullOrWhiteSpace(s)) YqStr += s + "&";

            }
            if (YqStr.StartsWith("&")) YqStr.Substring(1);

            returnStr = Helper.GenerateMD5(YqStr);

            return returnStr;
        }
        /// <summary>
        /// 去除特殊字符 可能存在解析错误
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string SafeInputStr(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                System.Text.RegularExpressions.Regex rule = new System.Text.RegularExpressions.Regex(@"[a-zA-Z0-9\u4e00-\u9fa5，,.。?？!！+-]+");

                string r = string.Empty;
                foreach (System.Text.RegularExpressions.Match mch in rule.Matches(str))
                {
                    r = r + mch.Value.Trim();
                }
                return r;
            }
            else
            {
                return " ";
            }

        }
        /// <summary>
        /// 去除特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeInput(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string except_chars = ": ‘ ！ @ # % … & * （  ^  &  ￥  ， 。 , .）$'";
                string GModelrep = System.Text.RegularExpressions.Regex.Replace(str, "[" + System.Text.RegularExpressions.Regex.Escape(except_chars) + "]", "");

                return GModelrep;
            }
            else
            {
                return " ";
            }

        }

        /// <summary>
        /// 将datatable转换为json  
        /// </summary>
        /// <param name="dtb">Dt</param>
        /// <returns>JSON字符串</returns>
        public static string Dtb2Json(DataTable dtb)
        {
            string JsonString = string.Empty;
            JsonString = JsonConvert.SerializeObject(dtb);
            return JsonString;
        }
        /// <summary>
        /// 转 decimal
        /// </summary>
        /// <param name="dtb"></param>
        /// <returns></returns>
        public static decimal strToDec(string str)
        {
            string reqStr = string.Empty;
            if (string.IsNullOrWhiteSpace(str)) reqStr = "0";
            else reqStr = str;
            decimal num = 0;

            if (!decimal.TryParse(reqStr, out num)) num = 0;
            return num;
        }

        /// <summary>
        /// 转 int
        /// </summary>
        /// <param name="dtb"></param>
        /// <returns></returns>
        public static int strToInt(string str)
        {
            string reqStr = string.Empty;
            if (string.IsNullOrWhiteSpace(str)) reqStr = "0";
            else reqStr = str;
            int num = 0;

            if (!int.TryParse(reqStr, out num)) num = 0;
            return num;
        }
         
        /// <summary>
        ///  自定义日志名称　记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public static void LogSQL(string name, string content)
        {
            try
            {
                string filePath_ml = AppDomain.CurrentDomain.BaseDirectory + "ServerLog";
                if (!Directory.Exists(filePath_ml))
                {
                    Directory.CreateDirectory(filePath_ml);

                }

                string filename = DateTime.Now.ToString("yyyyMMdd") + name + ".txt";

                string filePath = AppDomain.CurrentDomain.BaseDirectory + "ServerLog\\" + filename;
                FileInfo file = new FileInfo(filePath);

                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString());
                sb.Append(" ");
                sb.Append(content);
                FileMode fm = new FileMode();
                if (!file.Exists)
                {
                    fm = FileMode.Create;
                }
                else
                {
                    fm = FileMode.Append;
                }
                using (FileStream fs = new FileStream(filePath, fm, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {
                        //sw.WriteLine("******************************************");
                        sw.WriteLine(sb.ToString());

                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                //return;
            }
        }


        /// <summary>
        ///  字典排序
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public static string CreateSignDy(Dictionary<string, object> para)
        {
            StringBuilder Sign = new StringBuilder();
            string signtext = string.Empty;
            var items = para.OrderBy(o => o.Key, StringComparer.Ordinal);
            foreach (var item in items)
            {
                signtext += item.Key + item.Value;
            }
            //signtext = clientsecret + signtext + clientsecret;
            //var h = new HMACSHA256(Encoding.UTF8.GetBytes(clientsecret));
            //var sum = h.ComputeHash(Encoding.UTF8.GetBytes(signtext));
            //foreach (byte b in sum)
            //{
            //    Sign.Append(b.ToString("x2"));
            //}
            return signtext.ToString();
        }

        /// <summary>
        /// SHA512加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string sha512Encode(string source)
        {
            string result = "";
            byte[] buffer = Encoding.UTF8.GetBytes(source);//UTF-8 编码

            //64字节,512位
            SHA512CryptoServiceProvider SHA512 = new SHA512CryptoServiceProvider();
            byte[] h5 = SHA512.ComputeHash(buffer);

            result = BitConverter.ToString(h5).Replace("-", string.Empty);

            //return result.ToLower();
            return result.ToUpper();
        }

        /// <summary>
        /// json字符串将属性值中的英文一个单引号变为两个单引
        /// </summary>
        /// <param name="strJson">json字符串</param>
        /// <returns></returns>
        public static string JsonReplaceSign(string strJson)
        {
            if (strJson == "" || strJson == string.Empty || strJson == null)
            {

                return strJson;
            }
            //else if (strJson != "" || strJson != "")
            //{
            //    strJson = strJson.Replace("'", "''");
            //    return strJson.ToString();
            //}
            else
            {
                strJson = strJson.Replace("'", "''");
                return strJson.ToString();
            }

        }
        /// <summary>
        ///   sign加密
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="companyCode"></param>
        /// <param name="timestamps"></param>
        /// <param name="YourAppSecret"></param>
        /// <returns></returns>
        public static string JMJSON(string appkey, string companyCode, string timestamps, string YourAppSecret)
        {

            var sign = new Dictionary<string, object>();

            sign.Add("appKey", appkey);
            sign.Add("companyCode", companyCode);
            sign.Add("timestamp", timestamps);
            //排序
            string pjsign = CreateSignDy(sign);

            pjsign = YourAppSecret + pjsign + YourAppSecret;


            string forsigns = sha512Encode(pjsign);




            return forsigns;
        }

        /// <summary>
        /// 采购单2.0  sign加密
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="companyCode"></param>
        /// <param name="timestamps"></param>
        /// <param name="YourAppSecret"></param>
        /// <returns></returns>
        public static string getSign11(string timestamps)
        {
            Appinfo appinfo = new Appinfo();
            string platformKey = appinfo.platformKey;
            string platformSerect = appinfo.platformSerect;

            string pj = "platformKey=" + $"\"{platformKey}\"&" + "platformSerect=" + $"\"{platformSerect}\"&" + "timestamp=" + timestamps + "&";

            string sign = EncryptWithMD5(pj);

            return sign;
        }
        /// <summary>
        /// MD5　32位加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptWithMD5(string source)
        {
            byte[] sor = Encoding.UTF8.GetBytes(source);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                strbul.Append(result[i].ToString("X2"));//加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位

            }
            return strbul.ToString();
        }


        /// <summary>
        /// 获取当前时间戳 返回int 
        /// </summary>
        /// <returns></returns>
        public int GetTimeStamptoint()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            int asdasfasf = (int)Convert.ToInt64(ts.TotalSeconds);
            return asdasfasf;
        }
        /// <summary>
        /// 获得13位的时间戳
        /// </summary>
        /// <returns></returns>
        public static int GetTimeStamp()
        {
            System.DateTime time = System.DateTime.Now;
            long ts = ConvertDateTimeToInt(time);

            return (int)Convert.ToInt64(ts);
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        private static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        /// <summary>
        ///  字典转string
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="pageNo"></param>
        /// <param name="jmsign"></param>
        /// <param name="appkey"></param>
        /// <param name="timestamps"></param>
        /// <returns></returns>
        public string DictionaryTOstring(string companyCode, int pageNo, string jmsign, string appkey, string updateTime, string timestamps)
        {
            var json = new Dictionary<string, object>();
            json.Add("companyCode", companyCode);
            json.Add("version", "v2.0");
            json.Add("pageNo", pageNo);
            json.Add("sign", jmsign);
            json.Add("updateTime", updateTime);
            json.Add("pageSize", 100);
            json.Add("appKey", appkey);
            json.Add("timestamp", timestamps);

            string jsontostring = JsonConvert.SerializeObject(json);
            return jsontostring;
        }

        /// <summary>
        ///  new一个guid
        /// </summary>
        /// <returns></returns>
        public string GetGUID()
        {
            string str = Guid.NewGuid().ToString("N");

            return str.Substring(0, 15);
        }
    }
}

