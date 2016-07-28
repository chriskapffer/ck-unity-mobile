//
//  CKNetworkInfo.h
//  Copyright (c) 2015 ChrisKapffer. All rights reserved.
//

#import <Foundation/Foundation.h>

extern "C" {
    typedef void (*AccessTechnologyChangedCallback)(int);
}

@interface CKNetworkInfo : NSObject {
    
}

@property AccessTechnologyChangedCallback accessTechnologyChangedCallback;

+ (id)sharedInstance;
- (int)getCurrentAccessTechnology;
- (void)cleanup;

@end
