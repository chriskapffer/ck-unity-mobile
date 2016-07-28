//
//  CKSharingManager.mm
//  Copyright (c) 2015 ChrisKapffer. All rights reserved.
//

#import "CKSharingManager.h"

extern "C"
{
    void _Share(const char *text, const char *url, const void *imageData, uint imageSize, SharingFinishedCallback callback)
    {
        bool isShowing = [[CKSharingManager sharedInstance] isShowing];
        if (!isShowing) {
            [[CKSharingManager sharedInstance] shareWithText:[NSString stringWithUTF8String:text]
                                                         url:[NSString stringWithUTF8String:url]
                                                        data:[NSData dataWithBytes:imageData length:imageSize]
                                                    callback:callback];
        }
    }
}

@interface CKSharingManager()
{
    
}

@property SharingFinishedCallback finishedCallback;

@end

@implementation CKSharingManager

@synthesize isShowing;
@synthesize finishedCallback;

UIViewController *UnityGetGLViewController();

+ (id)sharedInstance
{
    static CKSharingManager *_instance;
    static dispatch_once_t _once_token;
    dispatch_once(&_once_token, ^ {
        _instance = [[CKSharingManager alloc] init];
    });
    return _instance;
}

- (id)init
{
    self = [super init];
    if (self) {
        self.isShowing = NO;
        self.finishedCallback = NULL;
    }
    return self;
}

- (void)shareWithText:(NSString *)text url:(NSString *)url data:(NSData *)data callback:(SharingFinishedCallback)callback
{
    if (self.isShowing) {
        return;
    }
    self.isShowing = YES;
    self.finishedCallback = callback;
    
    // append url to text
    if (text && url) {
        text = [text stringByAppendingFormat:@" %@", url];
    }
    
    NSArray *activityItems = NULL;
    if (data != NULL) {
        activityItems = [NSArray arrayWithObjects:text, [UIImage imageWithData:data], nil];
    } else {
        activityItems = [NSArray arrayWithObjects:text, nil];
    }

    UIActivityViewController *activityViewController = [[UIActivityViewController alloc] initWithActivityItems:activityItems applicationActivities:nil];
    activityViewController.modalTransitionStyle = UIModalTransitionStyleCoverVertical;
    activityViewController.excludedActivityTypes = @[UIActivityTypeAssignToContact,
                                                     UIActivityTypeAddToReadingList,
                                                     UIActivityTypePostToVimeo,
                                                     UIActivityTypeAirDrop];

    [activityViewController setCompletionHandler:^(NSString *activityType, BOOL completed) {
        self.isShowing = NO;
        if (self.finishedCallback != NULL) {
            self.finishedCallback([activityType UTF8String], completed);
        }
    }];
    
    if ( UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad )
    {
        UIView* view = UnityGetGLViewController().view;
        UIPopoverController* popover = [[UIPopoverController alloc] initWithContentViewController:activityViewController];
        [popover presentPopoverFromRect:CGRectMake(view.frame.size.width/2 - 50, view.frame.size.height/2, 100, 100) inView:view permittedArrowDirections:NULL animated:YES];
    } else {
        [UnityGetGLViewController() presentViewController:activityViewController animated:YES completion:NULL];
    }
}

@end
