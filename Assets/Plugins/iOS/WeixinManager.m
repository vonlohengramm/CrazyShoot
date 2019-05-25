//
//  WeixinManager.m
//  Unity-iPhone
//
//  Created by 张栩 on 16/4/13.
//
//

#import "WeixinManager.h"
#import "WXApi.h"

@implementation WeixinManager

void initWeiXin()
{
    [WXApi registerApp:@"wx0b13741d41728cdc"];
    if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 8.0)
    {
        UIUserNotificationType type = UIUserNotificationTypeBadge | UIUserNotificationTypeAlert | UIUserNotificationTypeSound;
        UIUserNotificationSettings *setting = [UIUserNotificationSettings settingsForTypes:type categories:nil];
        [[UIApplication sharedApplication] registerUserNotificationSettings:setting];
    }
}

-(BOOL)application:(UIApplication *)application handleOpenURL:(nonnull NSURL *)url
{
    return [WXApi handleOpenURL:url delegate:self];
}

-(BOOL)application:(UIApplication *)application openURL:(nonnull NSURL *)url sourceApplication:(nullable NSString *)sourceApplication annotation:(nonnull id)annotation
{
    return [WXApi handleOpenURL:url delegate:self];
}

void shareToWeiXin(void *p) {
    NSString *content = [NSString stringWithUTF8String:p];
    WXMediaMessage *message = [WXMediaMessage message];
    
    message.title = content;
    message.description = content;
    NSString *path = @"icon.png";
    
    WXWebpageObject *webpage = [WXWebpageObject object];
    webpage.webpageUrl = @"https://itunes.apple.com/cn/app/id1147038717";
    [message setThumbImage:[UIImage imageNamed:path]];
    message.mediaObject = webpage;
    
    SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
    req.bText = NO;
    req.message = message;
    req.scene = 1;
    [WXApi sendReq:req];
    
}

@end
