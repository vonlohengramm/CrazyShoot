//
//  WeixinManager.h
//  Unity-iPhone
//
//  Created by 张栩 on 16/4/13.
//
//

#import <Foundation/Foundation.h>
#import "WXApi.h"

@interface WeixinManager : NSObject<WXApiDelegate>

-(void)shareToWeiXin:(NSString *)param ;

@end
