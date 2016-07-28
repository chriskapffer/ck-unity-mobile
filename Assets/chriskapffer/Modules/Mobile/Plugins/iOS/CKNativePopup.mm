//
//  CKNativePopup.mm
//  Copyright (c) 2015 ChrisKapffer. All rights reserved.
//

#import "CKNativePopup.h"

extern "C"
{
    void _ShowPopup(const char *title, const char *message, const char *buttonTitles[], int buttonCount, PopupClosedCallback callback)
    {
        bool isShowing = [[CKNativePopup sharedInstance] isShowing];
        if (!isShowing) {
            NSMutableArray* buttons = [NSMutableArray arrayWithCapacity:buttonCount];
            for (int i = 0; i < buttonCount; i++) {
                [buttons addObject:[NSString stringWithUTF8String:buttonTitles[i]]];
            }
            
            [[CKNativePopup sharedInstance] showWithTitle:[NSString stringWithUTF8String:title]
                                                  message:[NSString stringWithUTF8String:message]
                                                  buttons:buttons
                                                 callback:callback];
        }
    }
}

@interface CKNativePopup()<UIAlertViewDelegate>
{
    
}

@property PopupClosedCallback closedCallback;

@end

@implementation CKNativePopup

@synthesize isShowing;
@synthesize closedCallback;

+ (id)sharedInstance
{
    static CKNativePopup *_instance;
    static dispatch_once_t _once_token;
    dispatch_once(&_once_token, ^ {
        _instance = [[CKNativePopup alloc] init];
    });
    return _instance;
}

- (id)init
{
    self = [super init];
    if (self) {
        self.isShowing = NO;
        self.closedCallback = NULL;
    }
    return self;
}

- (void)showWithTitle:(NSString*)title message:(NSString*)message buttons:(NSArray*)buttons callback:(PopupClosedCallback)callback
{
    if (self.isShowing) {
        return;
    }
    self.isShowing = YES;
    self.closedCallback = callback;
    
    UIAlertView *alert = [[UIAlertView alloc] init];
    [alert setTitle:title];
    [alert setMessage:message];
    [alert setDelegate:self];
    for (id buttonTitle in buttons) {
        [alert addButtonWithTitle:buttonTitle];
    }
    
    [alert show];
}

- (void)alertView:(UIAlertView *)alertView didDismissWithButtonIndex:(NSInteger)buttonIndex
{
    self.isShowing = NO;
    if (self.closedCallback != NULL) {
        self.closedCallback((int)buttonIndex);
    }
}

@end
