//
//  ShareManager.m
//  Unity-iPhone
//
//  Created by 张栩 on 16/1/21.
//
//

#import "ShareManager.h"
#import<Social/Social.h>


@implementation ShareManager

SLComposeViewController *composeVC;

void ShowShare(void *p)
{
    NSArray *contentArray = [[NSString stringWithUTF8String:p] componentsSeparatedByString:@"\t"];
    
    // 首先判断facebook分享是否可用
    if (![SLComposeViewController isAvailableForServiceType:SLServiceTypeFacebook]) {
        return;
    }
    // 创建控制器，并设置ServiceType
    composeVC = [SLComposeViewController composeViewControllerForServiceType:SLServiceTypeFacebook];
    // 添加要分享的图片
    [composeVC addImage:[UIImage imageWithContentsOfFile:contentArray[0]]];
    // 添加要分享的文字
    [composeVC setInitialText:contentArray[1]];
    // 添加要分享的url
    [composeVC addURL:[NSURL URLWithString:@"http://itunes.apple.com/app/id1147038717"]];
    // 弹出分享控制器
    //    UIViewController *appRootVC = [UIApplication sharedApplication].keyWindow.rootViewController;
    //    UIViewController *topVC = appRootVC;
    //    if (topVC.presentedViewController) {
    //        topVC = topVC.presentedViewController;
    //    }
    UIViewController *result = nil;
    
    UIWindow * window = [[UIApplication sharedApplication] keyWindow];
    if (window.windowLevel != UIWindowLevelNormal)
    {
        NSArray *windows = [[UIApplication sharedApplication] windows];
        for(UIWindow * tmpWin in windows)
        {
            if (tmpWin.windowLevel == UIWindowLevelNormal)
            {
                window = tmpWin;
                break;
            }
        }
    }
    
    UIView *frontView = [[window subviews] objectAtIndex:0];
    id nextResponder = [frontView nextResponder];
    
    if ([nextResponder isKindOfClass:[UIViewController class]])
        result = nextResponder;
    else
        result = window.rootViewController;
    
    [result presentViewController:composeVC animated:YES completion:nil];
    
    // 监听用户点击事件
    composeVC.completionHandler = ^(SLComposeViewControllerResult result){
        if (result == SLComposeViewControllerResultDone) {
            NSLog(@"点击了发送");
        }
        else if (result == SLComposeViewControllerResultCancelled)
        {
            NSLog(@"点击了取消");
        }
    };
}

void CloseShare()
{
    if (composeVC != nil)
    {
        [composeVC dismissViewControllerAnimated:true completion:^{
            composeVC = nil;
        }];
    }
}

@end
