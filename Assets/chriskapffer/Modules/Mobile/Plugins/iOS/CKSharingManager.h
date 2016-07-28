//
//  CKSharingManager.h
//  Copyright (c) 2015 ChrisKapffer. All rights reserved.
//

#import <Foundation/Foundation.h>

extern "C" {
    typedef void (*SharingFinishedCallback)(const char*, bool);
}

@interface CKSharingManager : NSObject {
    
}

@property BOOL isShowing;

+ (id)sharedInstance;

- (void)shareWithText:(NSString*)text url:(NSString*)url data:(NSData*)data callback:(SharingFinishedCallback)callback;

@end
