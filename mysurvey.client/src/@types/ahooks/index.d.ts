/*
 * @Author: 万雅虎
 * @Date: 2025-03-31 17:27:24
 * @LastEditTime: 2025-03-31 17:51:18
 * @LastEditors: 万雅虎
 * @Description: 
 * @FilePath: \MySurvey\mysurvey.client\src\@types\ahooks\index.d.ts
 * vipwan@sina.com © 万雅虎
 */
declare module 'ahooks' {
    import { DependencyList, EffectCallback, Dispatch, SetStateAction, RefObject, MutableRefObject } from 'react';

    // 基础 Hooks
    /** 组件挂载时执行的 Hook */
    export function useMount(fn: () => void | Promise<void>): void;

    /** 组件卸载时执行的 Hook */
    export function useUnmount(fn: () => void): void;

    /** 组件更新时执行的 Hook */
    export function useUpdate(): () => void;

    /** 组件挂载时执行的 Hook */
    export function useUpdateEffect(effect: EffectCallback, deps?: DependencyList): void;

    export function useUpdateLayoutEffect(effect: EffectCallback, deps?: DependencyList): void;
    /** 创建一个持久化的值，类似 useMemo，但不会在依赖变化时重新创建 */
    export function useCreation<T>(factory: () => T, deps: DependencyList): T;

    /** 类似于useState，但不会在依赖变化时重新创建 */
    export function useReactive<S extends object>(initialState: S): S;
    export function useLatest<T>(value: T): MutableRefObject<T>;
    export function useRafState<S>(initialState: S | (() => S)): [S, Dispatch<SetStateAction<S>>];
    export function useSafeState<S>(initialState: S | (() => S)): [S, Dispatch<SetStateAction<S>>];

    /** 用于在函数组件中获取组件实例  */
    export function useEventEmitter<T = void>(): {
        emit: (val: T) => void;
        useSubscription: (callback: (val: T) => void) => void;
    };

    // Dom 相关 Hooks
    export function useClickAway<T extends Element | Document = Element>(
        onClickAway: (event: MouseEvent | TouchEvent) => void,
        target: RefObject<T> | RefObject<T>[] | (() => T | T[] | null),
        options?: {
            eventName?: string | string[];
        }
    ): void;
    export function useDocumentVisibility(): DocumentVisibilityState;

    /** 拖拽 */
    export function useDrop<T>(options?: {
        onText?: (text: string, e: React.DragEvent) => void;
        onFiles?: (files: File[], e: React.DragEvent) => void;
        onUri?: (url: string, e: React.DragEvent) => void;
        onDom?: (content: T, e: React.DragEvent) => void;
        onDragEnter?: (e: React.DragEvent) => void;
        onDragOver?: (e: React.DragEvent) => void;
        onDragLeave?: (e: React.DragEvent) => void;
        onDrop?: (e: React.DragEvent) => void;
    }): {
        isHovering: boolean;
    } & { [key: string]: any };
    export function useDrag<T>(data: T, options?: {
        onDragStart?: (e: React.DragEvent) => void;
        onDragEnd?: (e: React.DragEvent) => void;
    }): { dragging: boolean } & { [key: string]: any };
    export function useEventListener(
        eventName: string,
        handler: (ev: Event) => void,
        options?: { target?: Element | Window | Document | null; capture?: boolean; once?: boolean; passive?: boolean }
    ): void;
    export function useExternal(path: string, options?: {
        type?: 'js' | 'css';
        js?: { async?: boolean; crossOrigin?: string };
        css?: { media?: string };
    }): 'unset' | 'loading' | 'ready' | 'error';
    export function useFavicon(href: string): void;
    export function useFullscreen(
        target: RefObject<Element> | Element,
        options?: {
            onExit?: () => void;
            onEnter?: () => void;
        }
    ): [boolean, { enter: () => void; exit: () => void; toggle: () => void }];
    export function useFocusWithin(
        target: RefObject<Element>,
        options?: {
            onFocus?: (e: FocusEvent) => void;
            onBlur?: (e: FocusEvent) => void;
            onChange?: (isFocusWithin: boolean) => void;
        }
    ): boolean;
    export function useHover(
        target: RefObject<Element>,
        options?: {
            onEnter?: () => void;
            onLeave?: () => void;
            onChange?: (isHovering: boolean) => void;
        }
    ): boolean;

    /**
     * 设置页面标题的 Hook
     * @param title
     * @param options
     */
    export function useTitle(title: string, options?: { restoreOnUnmount?: boolean }): void;
    export function useInViewport(
        target: RefObject<Element>,
        options?: {
            threshold?: number | number[];
            root?: RefObject<Element>;
            rootMargin?: string;
        }
    ): [boolean, IntersectionObserverEntry | undefined];

    /** 键盘事件 Hook */
    export function useKeyPress(
        keyFilter: string | string[] | ((event: KeyboardEvent) => boolean),
        eventHandler: (event: KeyboardEvent) => void,
        options?: {
            events?: string[];
            target?: Element | Window | Document | null;
        }
    ): void;

    /**
     * 长按事件 Hook
     * @param onLongPress
     * @param target
     * @param options
     */
    export function useLongPress(
        onLongPress: (e: MouseEvent | TouchEvent) => void,
        target: RefObject<Element>,
        options?: {
            delay?: number;
            moveThreshold?: { x?: number; y?: number };
            onClick?: (e: MouseEvent | TouchEvent) => void;
            onLongPressEnd?: (e: MouseEvent | TouchEvent) => void;
        }
    ): void;

    /**
     * 鼠标事件 Hook
     * @param target
     */
    export function useMouse(target?: RefObject<Element>): {
        elementX: number;
        elementY: number;
        elementPosX: number;
        elementPosY: number;
        screenX: number;
        screenY: number;
        clientX: number;
        clientY: number;
        pageX: number;
        pageY: number;
    };
    export function useResponsive(): {
        [key: string]: boolean;
    };
    /** 监听滚动位置的 Hook */
    export function useScroll(
        target?: RefObject<Element> | Document | Window,
        options?: {
            wait?: number;      // 防抖等待时间
            leading?: boolean;  // 是否在延迟开始前调用
            trailing?: boolean; // 是否在延迟结束后调用
        }
    ): { left: number; top: number };
    export function useSize(target: RefObject<Element>): { width: number; height: number };
    export function useTextSelection(): {
        text: string;
        rect: { top: number; left: number; bottom: number; right: number; height: number; width: number } | null;
    };

    // 高级 Hooks
    export interface CacheData<TData = any, TParams extends any[] = any> {
        data: TData;
        params: TParams;
        time: number;
    }

    /** 请求配置选项接口 */
    export interface Options<TData, TParams extends any[]> {
        manual?: boolean;           // 是否手动触发
        defaultParams?: TParams;    // 默认参数
        onBefore?: (params: TParams) => void;           // 请求前回调
        onSuccess?: (data: TData, params: TParams) => void;  // 请求成功回调
        onError?: (e: Error, params: TParams) => void;       // 请求失败回调
        onFinally?: (params: TParams, data?: TData, e?: Error) => void;  // 请求完成回调
        formatResult?: (res: any) => TData;                // 格式化响应数据
        pollingInterval?: number;   // 轮询间隔
        pollingWhenHidden?: boolean;  // 页面隐藏时是否继续轮询
        refreshOnWindowFocus?: boolean;  // 窗口聚焦时是否刷新
        focusTimespan?: number;      // 窗口聚焦时间阈值
        loadingDelay?: number;       // 加载延迟时间
        debounceInterval?: number;   // 防抖间隔
        throttleInterval?: number;   // 节流间隔
        initialData?: TData;         // 初始数据
        ready?: boolean;             // 是否准备就绪
        cacheKey?: string;           // 缓存键
        cacheTime?: number;          // 缓存时间
        staleTime?: number;          // 数据过期时间
        retryCount?: number;         // 重试次数
        retryInterval?: number;      // 重试间隔
        refreshDeps?: DependencyList;  // 刷新依赖
    }

    export interface FetchState<TData, TParams extends any[]> {
        loading: boolean;
        data?: TData;
        error?: Error;
        params: TParams | [];
    }

    /** 请求结果接口 */
    export interface Result<TData, TParams extends any[]> extends FetchState<TData, TParams> {
        run: (...params: TParams) => Promise<TData>;           // 手动触发请求
        runAsync: (...params: TParams) => Promise<TData>;      // 异步触发请求
        refresh: () => Promise<TData>;                         // 刷新当前请求
        refreshAsync: () => Promise<TData>;                    // 异步刷新当前请求
        mutate: (data?: TData | ((oldData?: TData) => TData | undefined)) => void;  // 修改数据
        cancel: () => void;                                    // 取消请求
    }

    export function useRequest<TData, TParams extends any[] = any[]>(
        service: (...args: TParams) => Promise<TData>,
        options?: Options<TData, TParams>
    ): Result<TData, TParams>;

    // 状态 Hooks
    /** 布尔值状态 Hook */
    export function useBoolean(defaultValue?: boolean): [
        boolean,
        {
            toggle: () => void;           // 切换布尔值
            set: (value: boolean) => void; // 设置布尔值
            setTrue: () => void;          // 设置为 true
            setFalse: () => void;         // 设置为 false
        }
    ];
    /** 切换状态 Hook */
    export function useToggle<T>(defaultValue: T): [T, (value?: T) => void];
    export function useToggle<T = boolean>(): [boolean, (value?: T) => void];
    export function useLocalStorageState<T>(
        key: string,
        options?: {
            defaultValue?: T | (() => T);
            serializer?: (value: T) => string;
            deserializer?: (value: string) => T;
        }
    ): [T | undefined, (value?: T | ((previousState: T) => T)) => void];
    export function useSessionStorageState<T>(
        key: string,
        options?: {
            defaultValue?: T | (() => T);
            serializer?: (value: T) => string;
            deserializer?: (value: string) => T;
        }
    ): [T | undefined, (value?: T | ((previousState: T) => T)) => void];
    export function useCookieState(
        key: string,
        options?: {
            defaultValue?: string | (() => string);
            expires?: number | Date;
            path?: string;
            domain?: string;
            secure?: boolean;
            sameSite?: 'strict' | 'lax' | 'none';
        }
    ): [string | undefined, (value?: string | ((previousState: string) => string)) => void];
    export function useDebounce<T>(
        value: T,
        options?: {
            wait?: number;
            leading?: boolean;
            trailing?: boolean;
            maxWait?: number;
        }
    ): T;
    export function useThrottle<T>(
        value: T,
        options?: {
            wait?: number;
            leading?: boolean;
            trailing?: boolean;
        }
    ): T;
    export function useMap<K, V>(initialValue?: Iterable<readonly [K, V]>): [
        Map<K, V>,
        {
            set: (key: K, value: V) => void;
            setAll: (newMap: Iterable<readonly [K, V]>) => void;
            remove: (key: K) => void;
            reset: () => void;
            get: (key: K) => V | undefined;
        }
    ];
    export function useSet<T>(initialValue?: Iterable<T>): [
        Set<T>,
        {
            add: (value: T) => void;
            remove: (value: T) => void;
            reset: () => void;
            has: (value: T) => boolean;
        }
    ];
    export function usePrevious<T>(state: T): T | undefined;
    export function useCounter(initialValue?: number, options?: {
        min?: number;
        max?: number;
    }): [
            number,
            {
                inc: (delta?: number) => void;
                dec: (delta?: number) => void;
                set: (value: number | ((c: number) => number)) => void;
                reset: () => void;
            }
        ];
    /** 获取最新状态的 Hook */
    export function useGetState<S>(initialState: S | (() => S)): [S, Dispatch<SetStateAction<S>>, () => S];

    // Effect Hooks
    /** 防抖 Effect Hook */
    export function useDebounceEffect(
        effect: EffectCallback,
        deps?: DependencyList,
        options?: {
            wait?: number;      // 防抖等待时间
            leading?: boolean;  // 是否在延迟开始前调用
            trailing?: boolean; // 是否在延迟结束后调用
            maxWait?: number;   // 最大等待时间
        }
    ): void;
    /** 防抖函数 Hook */
    export function useDebounceFn<T extends (...args: any[]) => any>(
        fn: T,
        options?: {
            wait?: number;      // 防抖等待时间
            leading?: boolean;  // 是否在延迟开始前调用
            trailing?: boolean; // 是否在延迟结束后调用
            maxWait?: number;   // 最大等待时间
        }
    ): {
        run: (...args: Parameters<T>) => ReturnType<T> | undefined;  // 执行函数
        cancel: () => void;                                          // 取消执行
        flush: () => ReturnType<T> | undefined;                      // 立即执行
    };
    export function useThrottleEffect(
        effect: EffectCallback,
        deps?: DependencyList,
        options?: {
            wait?: number;
            leading?: boolean;
            trailing?: boolean;
        }
    ): void;
    /** 节流函数 Hook */
    export function useThrottleFn<T extends (...args: any[]) => any>(
        fn: T,
        options?: {
            wait?: number;      // 节流等待时间
            leading?: boolean;  // 是否在延迟开始前调用
            trailing?: boolean; // 是否在延迟结束后调用
        }
    ): {
        run: (...args: Parameters<T>) => ReturnType<T> | undefined;  // 执行函数
        cancel: () => void;                                          // 取消执行
        flush: () => ReturnType<T> | undefined;                      // 立即执行
    };
    export function useInterval(
        fn: () => void,
        delay?: number | undefined | null,
        options?: {
            immediate?: boolean;
        }
    ): {
        clear: () => void;
    };
    export function useTimeout(
        fn: () => void,
        delay?: number | undefined | null,
        options?: {
            immediate?: boolean;
        }
    ): {
        clear: () => void;
    };
    /** 锁定函数 Hook，防止重复调用 */
    export function useLockFn<P extends any[] = any[], V = any>(fn: (...args: P) => Promise<V>): (...args: P) => Promise<V | undefined>;
    export function useAsyncEffect<T>(
        effect: () => Promise<T>,
        deps?: DependencyList
    ): void;

    // Scene Hooks

    export function useDynamicList<T>(initialList?: T[]): {
        list: T[];
        insert: (index: number, obj: T) => void;
        merge: (index: number, obj: Partial<T>) => void;
        replace: (index: number, obj: T) => void;
        remove: (index: number) => void;
        move: (fromIndex: number, toIndex: number) => void;
        getKey: (index: number) => number;
        getIndex: (key: number) => number;
        push: (obj: T) => void;
        pop: () => void;
        unshift: (obj: T) => void;
        shift: () => void;
        sort: (compareFn?: (a: T, b: T) => number) => void;
        reset: (newList: T[]) => void;
    };
    export function useVirtualList<T>(
        list: T[],
        options: {
            containerTarget: RefObject<Element>;
            wrapperTarget: RefObject<Element>;
            itemHeight: number | ((index: number, data: T) => number);
            overscan?: number;
        }
    ): {
        list: { data: T; index: number }[];
        scrollTo: (index: number) => void;
    };

    // 场景 Hooks 部分继续
    /**
     * 历史状态管理 Hook，支持撤销和重做
     * @param initialValue 初始值
     * @returns {
     *   value: 当前值,
     *   setValue: 设置新值,
     *   backLength: 可撤销的步数,
     *   forwardLength: 可重做的步数,
     *   go: 前进或后退指定步数,
     *   back: 撤销,
     *   forward: 重做,
     *   reset: 重置到指定值
     * }
     */
    export function useHistoryTravel<T>(initialValue?: T): {
        /** 当前值 */
        value: T;
        /** 设置新值 */
        setValue: (val: T) => void;
        /** 可撤销的步数 */
        backLength: number;
        /** 可重做的步数 */
        forwardLength: number;
        /** 前进或后退指定步数 */
        go: (step: number) => void;
        /** 撤销 */
        back: () => void;
        /** 重做 */
        forward: () => void;
        /** 重置到指定值 */
        reset: (val: T) => void;
    };

    /**
     * 网络连接状态 Hook
     * @returns {
     *   online: 是否在线,
     *   since: 状态变化时间,
     *   rtt: 网络往返时间,
     *   type: 网络类型,
     *   downlink: 下行速度,
     *   saveData: 是否启用省流量模式,
     *   downlinkMax: 最大下行速度,
     *   effectiveType: 网络类型(4g/3g等)
     * }
     */
    export function useNetwork(): {
        /** 是否在线 */
        online?: boolean;
        /** 状态变化时间 */
        since?: Date;
        /** 网络往返时间 */
        rtt?: number;
        /** 网络类型 */
        type?: string;
        /** 下行速度 */
        downlink?: number;
        /** 是否启用省流量模式 */
        saveData?: boolean;
        /** 最大下行速度 */
        downlinkMax?: number;
        /** 网络类型(4g/3g等) */
        effectiveType?: string;
    };

    /**
     * 列表选择管理 Hook
     * @param items 列表项数组
     * @param defaultSelected 默认选中项数组
     * @returns {
     *   selected: 已选项数组,
     *   allSelected: 是否全选,
     *   noneSelected: 是否未选择,
     *   partiallySelected: 是否部分选择,
     *   toggle: 切换选中状态,
     *   toggleAll: 切换全选,
     *   select: 选中项,
     *   unSelect: 取消选中,
     *   selectAll: 全选,
     *   unSelectAll: 取消全选,
     *   isSelected: 判断是否选中
     * }
     */
    export function useSelections<T>(
        items: T[],
        defaultSelected?: T[]
    ): {
        /** 已选项数组 */
        selected: T[];
        /** 是否全选 */
        allSelected: boolean;
        /** 是否未选择 */
        noneSelected: boolean;
        /** 是否部分选择 */
        partiallySelected: boolean;
        /** 切换选中状态 */
        toggle: (item: T) => void;
        /** 切换全选 */
        toggleAll: () => void;
        /** 选中项 */
        select: (item: T) => void;
        /** 取消选中 */
        unSelect: (item: T) => void;
        /** 全选 */
        selectAll: () => void;
        /** 取消全选 */
        unSelectAll: () => void;
        /** 判断是否选中 */
        isSelected: (item: T) => boolean;
    };

    /**
     * 倒计时 Hook
     * @param options 配置项
     * @param options.leftTime 剩余时间(毫秒)
     * @param options.targetDate 目标日期
     * @param options.interval 倒计时间隔(毫秒)
     * @param options.onEnd 倒计时结束回调
     * @returns 倒计时信息对象
     */
    export function useCountDown(options: {
        /** 剩余时间(毫秒) */
        leftTime?: number;
        /** 目标日期 */
        targetDate?: Date | number | string;
        /** 倒计时间隔(毫秒) */
        interval?: number;
        /** 倒计时结束回调 */
        onEnd?: () => void;
    }): {
        /** 剩余天数 */
        days: number;
        /** 剩余小时 */
        hours: number;
        /** 剩余分钟 */
        minutes: number;
        /** 剩余秒数 */
        seconds: number;
        /** 剩余毫秒 */
        milliseconds: number;
        /** 剩余总毫秒数 */
        total: number;
    };

    /**
     * 跟踪依赖变化的 Effect Hook
     * @param effect 副作用函数，接收依赖变化数组和新旧依赖值
     * @param deps 依赖数组
     */
    export function useTrackedEffect(
        effect: (
            /** 依赖变化状态数组 */
            changes: (boolean | undefined)[],
            /** 之前的依赖值 */
            previousDeps: DependencyList | undefined,
            /** 当前的依赖值 */
            currentDeps: DependencyList
        ) => void | (() => void),
        deps?: DependencyList
    ): void;

    /**
     * WebSocket 管理 Hook
     * @param socketUrl WebSocket 连接地址
     * @param options 配置选项
     * @returns WebSocket 控制对象
     */
    export function useWebSocket(
        socketUrl: string,
        options?: {
            /** 重连次数限制 */
            reconnectLimit?: number;
            /** 重连间隔(毫秒) */
            reconnectInterval?: number;
            /** 是否手动连接 */
            manual?: boolean;
            /** 连接建立时的回调 */
            onOpen?: (event: WebSocketEventMap['open']) => void;
            /** 连接关闭时的回调 */
            onClose?: (event: WebSocketEventMap['close']) => void;
            /** 收到消息的回调 */
            onMessage?: (event: WebSocketEventMap['message']) => void;
            /** 发生错误时的回调 */
            onError?: (event: WebSocketEventMap['error']) => void;
            /** WebSocket 协议 */
            protocols?: string | string[];
        }
    ): {
        /** 发送消息 */
        sendMessage: (message: string | ArrayBufferLike | Blob | ArrayBufferView) => void;
        /** 连接 WebSocket */
        connect: () => void;
        /** 断开连接 */
        disconnect: () => void;
        /** WebSocket 当前状态 */
        readyState: number;
        /** WebSocket 实例 */
        webSocketIns: WebSocket | undefined;
    };
}