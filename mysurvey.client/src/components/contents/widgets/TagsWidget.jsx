﻿import React, { useState, useEffect, useRef } from 'react';
import { Tag, Input, Tooltip, theme, message } from 'antd';
const { useToken } = theme;
import { PlusOutlined } from '@ant-design/icons';

// 自定义 Tags 编辑器组件
const TagsWidget = React.memo(({ onChange, value, schema }) => {
    const { token } = useToken();
    const [tags, setTags] = useState([]);
    const [inputVisible, setInputVisible] = useState(false);
    const [inputValue, setInputValue] = useState('');
    const [editInputIndex, setEditInputIndex] = useState(-1);
    const [editInputValue, setEditInputValue] = useState('');

    // 引用
    const inputRef = useRef(null);
    const editInputRef = useRef(null);

    // 验证输入是否为合法文本（只允许字母、数字、中文和部分常用标点）
    const isValidInput = (value) => {
        // 正则表达式匹配合法字符
        // 允许字母、数字、中文、空格和一些常用标点（如逗号、句号、短横线等）
        const validRegex = /^[\u4e00-\u9fa5a-zA-Z0-9\s.,，。、_\-]*$/;
        return validRegex.test(value);
    };

    // 初始化标签数据
    useEffect(() => {
        if (value) {
            try {
                // 尝试解析数组格式
                const parsedValue = Array.isArray(value)
                    ? value
                    : (typeof value === 'string' && value.includes(','))
                        ? value.split(',')
                        : [value];

                // 过滤掉空值并去重
                const filteredTags = [...new Set(parsedValue.filter(tag => tag && tag.trim()))];
                setTags(filteredTags);
            } catch (error) {
                console.error('解析标签值错误:', error);
                setTags([]);
            }
        } else {
            setTags([]);
        }
    }, [value]);

    // 聚焦到输入框
    useEffect(() => {
        if (inputVisible) {
            inputRef.current?.focus();
        }
    }, [inputVisible]);

    // 聚焦到编辑框
    useEffect(() => {
        if (editInputIndex !== -1) {
            editInputRef.current?.focus();
        }
    }, [editInputIndex]);

    // 处理标签变化，并通知父组件
    const handleTagsChange = (newTags) => {
        setTags(newTags);
        // 通知父组件值变化
        onChange(newTags);
    };

    // 处理关闭标签
    const handleClose = (removedTag) => {
        const newTags = tags.filter(tag => tag !== removedTag);
        handleTagsChange(newTags);
    };

    // 显示输入框
    const showInput = () => {
        setInputVisible(true);
    };

    // 处理输入框变化
    const handleInputChange = (e) => {
        const value = e.target.value;
        if (value && !isValidInput(value)) {
            message.warning('只能输入字母、数字、中文和常用标点');
            return;
        }
        setInputValue(value);
    };

    // 处理输入框确认
    const handleInputConfirm = () => {
        if (inputValue && !tags.includes(inputValue)) {
            if (isValidInput(inputValue)) {
                handleTagsChange([...tags, inputValue]);
            } else {
                message.error('标签包含不允许的字符');
            }
        }
        setInputVisible(false);
        setInputValue('');
    };

    // 处理开始编辑标签
    const handleEditInputChange = (e) => {
        const value = e.target.value;
        if (value && !isValidInput(value)) {
            message.warning('只能输入字母、数字、中文和常用标点');
            return;
        }
        setEditInputValue(value);
    };

    // 处理编辑框确认
    const handleEditInputConfirm = () => {
        const newTags = [...tags];
        // 检查标签是否合法及重复
        if (editInputValue && isValidInput(editInputValue) &&
            !tags.some((tag, i) => i !== editInputIndex && tag === editInputValue)) {
            newTags[editInputIndex] = editInputValue;
            handleTagsChange(newTags);
        } else if (!isValidInput(editInputValue)) {
            message.error('标签包含不允许的字符');
        }
        setEditInputIndex(-1);
        setEditInputValue('');
    };

    // 双击开始编辑标签
    const startEdit = (index) => {
        setEditInputIndex(index);
        setEditInputValue(tags[index]);
    };

    const tagPlusStyle = {
        borderStyle: 'dashed',
        cursor: 'pointer',
    };

    const tagInputStyle = {
        width: 78,
        marginRight: 8,
        verticalAlign: 'top',
    };

    // 读取schema中的属性配置
    const maxLength = schema?.props?.maxLength || 20; // 标签最大长度
    const maxCount = schema?.props?.maxCount || 10;   // 标签最大数量
    const placeholder = schema?.props?.placeholder || '按回车键添加';

    // 是否禁用新增标签的按钮（达到最大数量时）
    const isAddButtonDisabled = tags.length >= maxCount;

    return (
        <div className="tags-widget-container" style={{ marginBottom: 16 }}>
            {tags.map((tag, index) => {
                if (editInputIndex === index) {
                    return (
                        <Input
                            ref={editInputRef}
                            key={tag}
                            size="small"
                            style={tagInputStyle}
                            value={editInputValue}
                            maxLength={maxLength}
                            onChange={handleEditInputChange}
                            onBlur={handleEditInputConfirm}
                            onPressEnter={handleEditInputConfirm}
                        />
                    );
                }

                const isLongTag = tag.length > 20;
                const tagContent = isLongTag ? `${tag.slice(0, 20)}...` : tag;

                return (
                    <Tag
                        key={tag}
                        closable
                        color={schema?.props?.color || token.colorPrimary}
                        style={{ userSelect: 'none', marginBottom: 8 }}
                        onClose={() => handleClose(tag)}
                        onDoubleClick={() => startEdit(index)}
                    >
                        <span
                            onDoubleClick={(e) => {
                                e.preventDefault();
                                startEdit(index);
                            }}
                        >
                            {isLongTag ? (
                                <Tooltip title={tag}>
                                    {tagContent}
                                </Tooltip>
                            ) : (
                                tagContent
                            )}
                        </span>
                    </Tag>
                );
            })}

            {inputVisible ? (
                <Input
                    ref={inputRef}
                    type="text"
                    size="small"
                    style={tagInputStyle}
                    value={inputValue}
                    maxLength={maxLength}
                    onChange={handleInputChange}
                    onBlur={handleInputConfirm}
                    onPressEnter={handleInputConfirm}
                    placeholder={placeholder}
                />
            ) : (
                <Tooltip
                    title={isAddButtonDisabled ? `最多只能添加${maxCount}个标签` : '点击添加标签'}
                >
                    <Tag
                        onClick={isAddButtonDisabled ? undefined : showInput}
                        style={{
                            ...tagPlusStyle,
                            backgroundColor: isAddButtonDisabled ? token.colorBgContainerDisabled : undefined,
                            color: isAddButtonDisabled ? token.colorTextDisabled : undefined,
                            cursor: isAddButtonDisabled ? 'not-allowed' : 'pointer',
                            marginBottom: 8
                        }}
                        icon={<PlusOutlined />}
                    >
                        添加标签
                    </Tag>
                </Tooltip>
            )}

            <div style={{ marginTop: 8, fontSize: 12, color: token.colorTextSecondary }}>
                {schema?.description ? (
                    <div>{schema.description}</div>
                ) : (
                    <div>提示：双击标签可编辑，按回车确认；仅允许输入字母、数字、中文和常用标点</div>
                )}
                <div>当前：{tags.length}/{maxCount}</div>
            </div>
        </div>
    );
});

export default TagsWidget;
