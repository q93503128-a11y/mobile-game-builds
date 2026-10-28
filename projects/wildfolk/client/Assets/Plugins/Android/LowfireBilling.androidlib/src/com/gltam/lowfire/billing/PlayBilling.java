package com.gltam.lowfire.billing;

import android.app.Activity;
import com.android.billingclient.api.*;
import java.util.*;
import org.json.*;

/** No wallet grants or client consumption. All outbound diagnostics are safe codes. */
public final class PlayBilling implements PurchasesUpdatedListener {
    public interface Callback { void onResult(String json); }
    private final Activity activity;
    private final Callback callback;
    private final BillingClient client;
    private final Map<String,ProductDetails> products=new HashMap<>();
    private final Map<String,ProductDetails.OneTimePurchaseOfferDetails> offers=new HashMap<>();
    private boolean disposed,launching;
    public PlayBilling(Activity activity,Callback callback) {
        this.activity=activity;this.callback=callback;
        client=BillingClient.newBuilder(activity).setListener(this)
            .enablePendingPurchases(PendingPurchasesParams.newBuilder().enableOneTimeProducts().build())
            .enableAutoServiceReconnection().build();
    }
    private void send(String type,String request,int code,JSONArray items) {
        if(disposed)return;
        try {JSONObject out=new JSONObject();out.put("Type",type);out.put("Request",request);out.put("Code",code);out.put("Items",items==null?new JSONArray():items);callback.onResult(out.toString());}
        catch(Exception ignored) { /* no raw purchase/error logging */ }
    }
    private void connected(String type,String request,Runnable action) {
        activity.runOnUiThread(()->{
            if(disposed){return;}if(client.isReady()){action.run();return;}
            client.startConnection(new BillingClientStateListener(){
                public void onBillingSetupFinished(BillingResult result){if(disposed)return;if(result.getResponseCode()==BillingClient.BillingResponseCode.OK)action.run();else send(type,request,result.getResponseCode(),null);}
                public void onBillingServiceDisconnected(){products.clear();offers.clear();send("disconnected","",BillingClient.BillingResponseCode.SERVICE_DISCONNECTED,null);}
            });
        });
    }
    public void fetch(String request,String jsonIds) {
        products.clear();offers.clear();
        connected("products",request,()->{
            try {
                JSONArray ids=new JSONArray(jsonIds);if(ids.length()<1||ids.length()>5){send("products",request,6,null);return;}
                List<QueryProductDetailsParams.Product> query=new ArrayList<>();
                for(int i=0;i<ids.length();i++)query.add(QueryProductDetailsParams.Product.newBuilder().setProductId(ids.getString(i)).setProductType(BillingClient.ProductType.INAPP).build());
                client.queryProductDetailsAsync(QueryProductDetailsParams.newBuilder().setProductList(query).build(),(result,data)->{
                    JSONArray rows=new JSONArray();if(disposed)return;
                    if(result.getResponseCode()==BillingClient.BillingResponseCode.OK)for(ProductDetails p:data.getProductDetailsList())try {
                        List<ProductDetails.OneTimePurchaseOfferDetails> choices=p.getOneTimePurchaseOfferDetailsList();
                        if(choices==null)continue;
                        ProductDetails.OneTimePurchaseOfferDetails offer=null;
                        for(ProductDetails.OneTimePurchaseOfferDetails candidate:choices)
                            if(candidate.getOfferId()==null&&candidate.getRentalDetails()==null&&candidate.getPreorderDetails()==null){if(offer!=null){offer=null;break;}offer=candidate;}
                        if(offer==null)continue;products.put(p.getProductId(),p);offers.put(p.getProductId(),offer);
                        JSONObject row=new JSONObject();row.put("ProductId",p.getProductId());row.put("FormattedPrice",offer.getFormattedPrice());row.put("Currency",offer.getPriceCurrencyCode());row.put("PriceMicros",offer.getPriceAmountMicros());rows.put(row);
                    }catch(Exception ignored){}
                    send("products",request,result.getResponseCode(),rows);
                });
            }catch(Exception ignored){send("products",request,6,null);}
        });
    }
    private JSONArray purchases(List<Purchase> values) {
        JSONArray rows=new JSONArray();if(values==null)return rows;
        for(Purchase p:values)try {
            if(p.getProducts().size()!=1||p.getQuantity()!=1)continue;
            int state=p.getPurchaseState();if(state!=Purchase.PurchaseState.PURCHASED&&state!=Purchase.PurchaseState.PENDING)continue;
            AccountIdentifiers ids=p.getAccountIdentifiers();if(ids==null)continue;
            JSONObject row=new JSONObject();row.put("ProductId",p.getProducts().get(0));row.put("Token",p.getPurchaseToken());row.put("Quantity",p.getQuantity());row.put("AccountBinding",ids.getObfuscatedAccountId());row.put("ProfileBinding",ids.getObfuscatedProfileId());row.put("Pending",state==Purchase.PurchaseState.PENDING);rows.put(row);
        }catch(Exception ignored){}
        return rows;
    }
    public void owned(String request) {
        connected("owned",request,()->client.queryPurchasesAsync(QueryPurchasesParams.newBuilder().setProductType(BillingClient.ProductType.INAPP).build(),(result,values)->send("owned",request,result.getResponseCode(),purchases(values))));
    }
    public void buy(String product,String account,String profile) {
        activity.runOnUiThread(()->{
            if(disposed)return;
            if(launching||!client.isReady()||!products.containsKey(product)||!offers.containsKey(product)||!account.matches("[a-f0-9]{64}")||!profile.matches("[a-f0-9]{64}")){send("purchase","",6,null);return;}
            launching=true;
            try {
                BillingFlowParams.ProductDetailsParams details=BillingFlowParams.ProductDetailsParams.newBuilder().setProductDetails(products.get(product)).setOfferToken(offers.get(product).getOfferToken()).build();
                BillingResult result=client.launchBillingFlow(activity,BillingFlowParams.newBuilder().setProductDetailsParamsList(Collections.singletonList(details)).setObfuscatedAccountId(account).setObfuscatedProfileId(profile).build());
                if(result.getResponseCode()!=BillingClient.BillingResponseCode.OK){launching=false;send("purchase","",result.getResponseCode(),null);}
            }catch(Exception ignored){launching=false;send("purchase","",6,null);}
        });
    }
    @Override public void onPurchasesUpdated(BillingResult result,List<Purchase> purchases) {launching=false;send("purchase","",result.getResponseCode(),purchases(purchases));}
    public void dispose(){disposed=true;client.endConnection();products.clear();offers.clear();}
}
