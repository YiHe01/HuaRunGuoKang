using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanjingGY
{
    public class jsonClass
    {
        public class Root
        {
            public string SER_NUMBER { get; set; }
            public List<DataItem> data { get; set; }

        }
        public class DataItem
        {

            public string GOODS_ID { get; set; }
            public string GOODS_MAIN { get; set; }
            public string GOODSSYS_ID { get; set; }
            public string GOODS_NAME { get; set; }
            public string CLASS_CODE { get; set; }
            public string UNIT_ID { get; set; }
            public string UNIT_NAME { get; set; }
            public string GOODS_SPEC { get; set; }
            public string MAKER_ID { get; set; }
            public string MAKER_NAME { get; set; }
            public string GOODS_TYPE { get; set; }
            public string QUERY_ID { get; set; }
            public string LICENSE { get; set; }
            public double RATIO { get; set; }
            public double RATIO1 { get; set; }

            public int ISDELETED { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_STATE { get; set; }
            public string OPT_DATE { get; set; }
            public int ROW_ID { get; set; }
            public string CADN_NAME { get; set; }
            public string CLIENT_ID { get; set; }
            public string CLIENTDIS_ID { get; set; }
            public string CLASS_ID { get; set; }
            public string NAME { get; set; }
            public string LEVELER { get; set; }
            public string ADDRESS { get; set; }
            public string CONTACT { get; set; }
            public string QUERYID { get; set; }
            public string DELIVER_ADDR { get; set; }

            public int BILL_FLAG { get; set; }
            public string SUPPLIER_ID { get; set; }
            public string SUPPLIERDIS_ID { get; set; }
            public string SUPPLIER_NAME { get; set; }

            public string ADD_ID { get; set; }

            public string CLIENTSYS_ID { get; set; }

            public string REMARK { get; set; }

            public string OWNER_ADDRESS { get; set; }
            public string OWNER_NAME { get; set; }
            public string PROV_CODE { get; set; }
            public string CITY_CODE { get; set; }
            public string AREA_COD { get; set; }
            public string SORT_ID { get; set; }
            public string SORT_FLAG { get; set; }

            public string SORT_NAME { get; set; }
            public string CREATE_DATE { get; set; }
            public string CREATE_MAN { get; set; }

            public string UPDSTAT { get; set; }

            public string LICENSE_STYLE { get; set; }

            public string LICENSE_ID { get; set; }
            public string LICENSE_DESCRIPTION { get; set; }
            public string RELEASE_DATE { get; set; }
            public string VALID_DATE { get; set; }
            public string CHECK_STYLE { get; set; }


        }


        //商品信息表
        public class ERPGOODS_INF_nr
        {


            public string GOODS_ID { get; set; }
            public string GOODS_MAIN { get; set; }
            public string GOODSSYS_ID { get; set; }
            public string GOODS_NAME { get; set; }
            public string CLASS_CODE { get; set; }
            public string UNIT_ID { get; set; }
            public string UNIT_NAME { get; set; }
            public string GOODS_SPEC { get; set; }
            public string MAKER_ID { get; set; }
            public string MAKER_NAME { get; set; }
            public string GOODS_TYPE { get; set; }
            public string QUERY_ID { get; set; }
            public string LICENSE { get; set; }
            public double RATIO { get; set; }
            public double RATIO1 { get; set; }

            public int ISDELETED { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_STATE { get; set; }
            public string OPT_DATE { get; set; }
            public int ROW_ID { get; set; }
            public string CADN_NAME { get; set; }





        }

        public class ERPCLIENT_nr
        {
            public string CLIENT_ID { get; set; }
            public string CLIENTDIS_ID { get; set; }
            public string CLASS_ID { get; set; }
            public string NAME { get; set; }
            public string LEVELER { get; set; }
            public string ADDRESS { get; set; }
            public string CONTACT { get; set; }
            public string QUERYID { get; set; }
            public string DELIVER_ADDR { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_STATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_DATE { get; set; }
            public string OPT_DATE { get; set; }
            public int ROW_ID { get; set; }
            public int BILL_FLAG { get; set; }


        }
        public class ERPSUPPLIER_INF_nr
        {
            public string SUPPLIER_ID { get; set; }
            public string SUPPLIERDIS_ID { get; set; }
            public string SUPPLIER_NAME { get; set; }
            public string QUERY_ID { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string OPT_DATE { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_STATE { get; set; }
            public int ROW_ID { get; set; }

        }
        public class ERPCLIENT_ADDRESS_nr
        {
            public string ADD_ID { get; set; }
            public string CLIENT_ID { get; set; }
            public string CLIENTSYS_ID { get; set; }
            public string ADDRESS { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_STATE { get; set; }
            public string UPD_MODE { get; set; }
            public string REMARK { get; set; }
            public int ROW_ID { get; set; }
            public string OWNER_ADDRESS { get; set; }
            public string OWNER_NAME { get; set; }
            public string PROV_CODE { get; set; }
            public string CITY_CODE { get; set; }
            public string AREA_COD { get; set; }





        }
        public class ERPGOODS_SORT_nr
        {
            public string SORT_ID { get; set; }
            public string SORT_FLAG { get; set; }
            public string GOODS_ID { get; set; }
            public string SORT_NAME { get; set; }
            public string CREATE_DATE { get; set; }
            public string CREATE_MAN { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPDSTAT { get; set; }
            public int ROW_ID { get; set; }

        }
        public class ERPLICENSE_INF_nr
        {
            public int ROW_ID { get; set; }
            public string LICENSE_STYLE { get; set; }
            public string UNIT_ID { get; set; }
            public string LICENSE_ID { get; set; }
            public string LICENSE_DESCRIPTION { get; set; }
            public string RELEASE_DATE { get; set; }
            public string VALID_DATE { get; set; }
            public string CHECK_STYLE { get; set; }
            public string CREATE_MAN { get; set; }
            public string CREATE_DATE { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_STATE { get; set; }
            public string OPT_DATE { get; set; }


        }

        public class DataItem_201
        {
            // public class ERPSALEPROOF_nr
            public string PROOF_ID { get; set; }
            public string SRC_PROOF_ID { get; set; }
            public string SRC_LINE_ID { get; set; }
            public string SALE_TYPE { get; set; }
            public string PROOF_DATE { get; set; }
            public string CUSTOMER_ID { get; set; }
            public string CUSTOMERSYS_ID { get; set; }
            public string GOODS_ID { get; set; }
            public string GOODSSYS_ID { get; set; }
            public string BATCH_NO { get; set; }
            public string VALID_DATE { get; set; }
            public string PRODUCT_DATE { get; set; }
            public string QUANTITY { get; set; }
            public string PRICE { get; set; }
            public string GOODS_AMOUNT { get; set; }
            public string WS_PRICE { get; set; }
            public string RT_PRICE { get; set; }
            public string BUY_PRICE { get; set; }
            public string WAREHOST_ID { get; set; }
            public string WAREHOUSE_TYPE { get; set; }
            public string PROOF_HUMAN { get; set; }
            public string BILL_LEVEL { get; set; }
            public string COMPANY_ID { get; set; }
            public string DEPART_ID2 { get; set; }
            public string MONOMAR { get; set; }
            public string YAOJIAN_FLAG { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_STATE { get; set; }
            public string MEMO_FILE1 { get; set; }
            public string ROW_ID { get; set; }


        }

        public class Root_201
        {
            public string SER_NUMBER { get; set; }
            public List<DataItem_201> data { get; set; }

        }

        public class DataItem_202
        {
            // public class ERPBUYPLAN_nr
            public string SRC_PROOF_ID { get; set; }
            public string SRC_LINE_ID { get; set; }
            public string SUPPLIER_ID { get; set; }
            public string GOODS_ID { get; set; }
            public string BATCH_NO { get; set; }
            public string VALID_DATE { get; set; }
            public string PRODUCT_DATE { get; set; }
            public string BUY_PRICE { get; set; }
            public string QUANTITY { get; set; }
            public string PIECE { get; set; }
            public string AMOUNT { get; set; }
            public string PROOF_MAN { get; set; }
            public string PROOF_DATE { get; set; }
            public string ORDER_FLAG { get; set; }
            public string RECEIVE_TYPE { get; set; }
            public string STOCKCALGROUP_ID { get; set; }
            public string CREATE_MAN { get; set; }
            public string CREATE_DATE { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_DATE { get; set; }
            public string UPD_STATE { get; set; }
            public string UPD_MODE { get; set; }
            public string OPT_DATE { get; set; }
            public string ROW_ID { get; set; }



        }
        public class Root_202
        {
            public string SER_NUMBER { get; set; }
            public List<DataItem_202> data { get; set; }

        }


        public class DataItem_204
        {
            // public class ERPBUYPLAN_nr
            public string SRC_PROOF_ID { get; set; }
            public string SRC_LINE_ID { get; set; }
            public string MOVE_TYPE { get; set; }
            public string SRC_STOCKCALGROUP_ID { get; set; }
            public string DEST_STOCKCALGROUP_ID { get; set; }
            public string GOODS_ID { get; set; }
            public string VALID_DATE { get; set; }
            public string PRODUCT_DATE { get; set; }
            public string BATCH_NO { get; set; }
            public string CREATE_MAN { get; set; }
            public string CREATE_DATE { get; set; }
            public string SRC_WAREHOUSE_TYPE { get; set; }
            public string QUANTITY { get; set; }
            public string DEST_WAREHOUSE_TYPE { get; set; }
            public string STATE { get; set; }
            public string ORG_ID { get; set; }
            public string OWNER_ID { get; set; }
            public string CREATE_ORG { get; set; }
            public string CREATE_ORGSEQ { get; set; }
            public string UPD_STATE { get; set; }
            public string UPD_MODE { get; set; }
            public string UPD_DATE { get; set; }
            public string OPT_DATE { get; set; }
            public string ROW_ID { get; set; }



        }
        public class Root_204
        {
            public string SER_NUMBER { get; set; }
            public List<DataItem_204> data { get; set; }

        }

    }
}
