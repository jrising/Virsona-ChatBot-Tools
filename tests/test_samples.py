#This module tests James Rising's text processing beast using two files containing sample sentences and templates respectively.

import os

templates=list(eval(open("template.txt").read()))
texts=list(eval(open('sample.txt').read()))
text_templates=zip(texts, templates)


expressions=['''mono ../../DataTemple/bin/DataTemple.exe -c ../../DataTemple/config.xml -P "%s" -T "%s" -O "%s" -I "%s"''' % (temp[0], temp[1], temp[2], text) for text, temp in text_templates]

for expr in expressions:
    print expr
    os.system(expr)

