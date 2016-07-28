//
//  CKNativePopup.h
//  Copyright (c) 2015 ChrisKapffer. All rights reserved.
//

#import <Foundation/Foundation.h>

extern "C" {
    typedef void (*PopupClosedCallback)(int);
}

@interface CKNativePopup : NSObject {
    
}

@property BOOL isShowing;

+ (id)sharedInstance;

- (void)showWithTitle:(NSString*)title message:(NSString*)message buttons:(NSArray*)buttons callback:(PopupClosedCallback)callback;

@end
