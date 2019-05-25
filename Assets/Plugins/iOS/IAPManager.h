//
//  IapManager.h
//  Unity-iPhone
//
//  Created by 张栩 on 15/11/29.
//
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

@interface IAPManager : NSObject<SKProductsRequestDelegate, SKPaymentTransactionObserver>{
    SKProduct *proUpgradeProduct;
    SKProductsRequest *productsRequest;
    NSArray *products;
}

-(void)attachObserver;
-(BOOL)CanMakePayment;
-(void)requestProductData:(NSString *)productIdentifiers;
-(void)buyRequest:(NSString *)productIdentifier;

@end
