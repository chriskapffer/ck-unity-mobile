//
//  CKNetworkInfo.mm
//  Copyright (c) 2015 ChrisKapffer. All rights reserved.
//

#import <CoreTelephony/CTTelephonyNetworkInfo.h>

#import "CKNetworkInfo.h"

extern "C"
{
    void _RegisterNetworkTypeChangedCallback(AccessTechnologyChangedCallback callback)
    {
        [[CKNetworkInfo sharedInstance] setAccessTechnologyChangedCallback:callback];
    }
    
    int _GetCurrentNetworkType()
    {
        return [[CKNetworkInfo sharedInstance] getCurrentAccessTechnology];
    }
    
    void _CleanupResources()
    {
        return [[CKNetworkInfo sharedInstance] cleanup];
    }
}

@interface CKNetworkInfo()
{
    
}

@property BOOL canReadRadioAccess;
@property (strong) NSDictionary *accessTechnologyTypes;
@property (strong) CTTelephonyNetworkInfo *telephonyInfo;

@end

@implementation CKNetworkInfo

@synthesize canReadRadioAccess;
@synthesize accessTechnologyChangedCallback;
@synthesize accessTechnologyTypes;
@synthesize telephonyInfo;

+ (id)sharedInstance
{
    static CKNetworkInfo *_instance;
    static dispatch_once_t _once_token;
    dispatch_once(&_once_token, ^ {
        _instance = [[CKNetworkInfo alloc] init];
    });
    return _instance;
}

- (id)init
{
    self = [super init];
    if (self) {
        self.telephonyInfo = [[CTTelephonyNetworkInfo alloc] init];
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 70000
        self.canReadRadioAccess = [self.telephonyInfo respondsToSelector:@selector(currentRadioAccessTechnology)];
        if (self.canReadRadioAccess) {
            self.accessTechnologyTypes = [NSDictionary dictionaryWithObjectsAndKeys:
                                          [NSNumber numberWithInt: 1], CTRadioAccessTechnologyCDMA1x,
                                          [NSNumber numberWithInt: 3], CTRadioAccessTechnologyGPRS,
                                          [NSNumber numberWithInt: 4], CTRadioAccessTechnologyEdge,
                                          [NSNumber numberWithInt: 5], CTRadioAccessTechnologyWCDMA,
                                          [NSNumber numberWithInt: 6], CTRadioAccessTechnologyCDMAEVDORev0,
                                          [NSNumber numberWithInt: 7], CTRadioAccessTechnologyCDMAEVDORevA,
                                          [NSNumber numberWithInt: 8], CTRadioAccessTechnologyCDMAEVDORevB,
                                          [NSNumber numberWithInt: 9], CTRadioAccessTechnologyeHRPD,
                                          [NSNumber numberWithInt:11], CTRadioAccessTechnologyHSDPA,
                                          [NSNumber numberWithInt:12], CTRadioAccessTechnologyHSUPA,
                                          [NSNumber numberWithInt:14], CTRadioAccessTechnologyLTE,
                                          nil];
            [NSNotificationCenter.defaultCenter addObserver:self selector:@selector(accessTechnologyDidChange) name:CTRadioAccessTechnologyDidChangeNotification object:nil];
        }
#endif
    }
    return self;
}

- (void)dealloc
{
    [self cleanup];
}

- (void)accessTechnologyDidChange
{
    self.accessTechnologyChangedCallback([self getCurrentAccessTechnology]);
}

- (int)getCurrentAccessTechnology
{
    int failure = -1;
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 70000
    if (!self.canReadRadioAccess) {
        return failure;
    }
    
    NSString *current = self.telephonyInfo.currentRadioAccessTechnology;
    NSNumber *number = [self.accessTechnologyTypes objectForKey:current];
    if(number == nil) {
        return failure;
    }
    return [number intValue];
#else
    return failure;
#endif
}

- (void)cleanup
{
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 70000
    if (self.canReadRadioAccess) {
        [NSNotificationCenter.defaultCenter removeObserver:self name:CTRadioAccessTechnologyDidChangeNotification object:nil];
    }
#endif
}

@end
